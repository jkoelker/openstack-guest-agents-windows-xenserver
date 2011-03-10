/*
 * vim: tabstop=4 shiftwidth=4 softtabstop=4
 *
 * Copyright (c) 2011 Openstack, LLC.
 * All Rights Reserved.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License"); you may
 *    not use this file except in compliance with the License. You may obtain
 *    a copy of the License at
 *
 *         http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 *    WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 *    License for the specific language governing permissions and limitations
 *    under the License.
 */

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <dlfcn.h>
#include <sys/types.h>
#include <signal.h>
#include <dlfcn.h>
#include <string.h>
#include <assert.h>
#include <errno.h>
#include "python.h"
#include "plugin_int.h"
#include "nova-agent.h"

#define AGENT_MODULE_NAME "nova_agent"
#define PLUGIN_METATYPE_NAME "plugin_metatype"
#define PLUGIN_CLASS_NAME "plugin"
#define PLUGIN_INFO_NAME "plugin_info"
#define PLUGIN_INFO_LIST_NAME "plugin_info_list"

/* 
 * Double underscore would be enough to keep it from being accessed
 * like: PLUGIN_CLASS_NAME.<attribute>.  But, it's even more interesting
 * to put a dot in the name.  Note that getattr() will still find it,
 * of course, and it'll show up in dir().
 */
#define PLUGINS_MANGLED_ATTRIBUTE "_plugins.info"


typedef struct _agent_plugin agent_plugin_t;
typedef struct _agent_plugin_list agent_plugin_list_t;
typedef struct _agent_metaclass_info agent_metaclass_info_t;

struct _agent_plugin
{
    pthread_t thr_id;
    char *name;
    PyObject *cls;
    PyObject *parser; /* only may be set for exchange plugins */
    agent_plugin_t *next;
};

static PyTypeObject agent_plugin_metatype = 
{
    PyVarObject_HEAD_INIT(NULL, 0)
    AGENT_MODULE_NAME "." PLUGIN_METATYPE_NAME, /*tp_name*/
};

static PyTypeObject agent_plugin_class = 
{
    PyVarObject_HEAD_INIT(&agent_plugin_metatype, 0)
    AGENT_MODULE_NAME "." PLUGIN_CLASS_NAME, /*tp_name*/
};

static pthread_mutex_t _plugins_lock;
static int _plugins_running = 0;
static int _plugins_die = 0;
static agent_plugin_t *_parser_plugins = NULL;
static agent_plugin_t *_exchange_plugins = NULL;

static void *_agent_parser_plugin_thread(void *arg)
{
    agent_plugin_t *plugin = arg;
    PyObject *e_instance;
    PyObject *p_instance;
    PyGILState_STATE gstate;
    PyObject *get_req;
    PyObject *put_resp;
    PyObject *parse;
    PyObject *req;
    PyObject *resp;


    pthread_mutex_lock(&_plugins_lock);

    if (_plugins_running == 0)
    {
        pthread_mutex_unlock(&_plugins_lock);
        return NULL;
    }

    pthread_mutex_unlock(&_plugins_lock);

    /* Acquire GIL */
    gstate = PyGILState_Ensure();

    e_instance = PyObject_CallFunctionObjArgs(plugin->cls, NULL);
    if (e_instance == NULL)
    {
        PyErr_Print();
        PyGILState_Release(gstate);
        kill(getpid(), SIGTERM);
        return NULL;
    }

    p_instance = PyObject_CallFunctionObjArgs(plugin->parser, NULL);
    if (p_instance == NULL)
    {
        PyErr_Print();
        PyGILState_Release(gstate);
        kill(getpid(), SIGTERM);
        return NULL;
    }

    get_req = PyObject_GetAttrString(plugin->cls, "get_request");

    put_resp = PyObject_GetAttrString(plugin->cls, "put_response");

    parse = PyObject_GetAttrString(plugin->parser, "parse_request");

    PyGILState_Release(gstate);

    pthread_setcancelstate(PTHREAD_CANCEL_DISABLE, NULL);

    for(;;)
    {
        /* check for shut down */

        gstate = PyGILState_Ensure();

        if (_plugins_die)
        {
            break;
        }

        req = PyObject_CallFunctionObjArgs(get_req, e_instance, NULL);
        if ((req == NULL) || (req == Py_None))
        {
            Py_XDECREF(req);
            PyGILState_Release(gstate);
            sleep(1);
            continue;
        }

        resp = PyObject_CallFunctionObjArgs(parse, p_instance, req, NULL);
        if (resp == NULL)
        {
            PyErr_Print();
            Py_DECREF(req);
            Py_XDECREF(resp);
            PyGILState_Release(gstate);
            continue;
        }

        PyObject_CallFunctionObjArgs(put_resp, e_instance, req, resp, NULL);
        if (PyErr_Occurred())
        {
            PyErr_Print();
        }

        Py_DECREF(req);
        Py_DECREF(resp);

        PyGILState_Release(gstate);
    }

    Py_DECREF(get_req);
    Py_DECREF(put_resp);
    Py_DECREF(parse);

    Py_DECREF(p_instance);
    Py_DECREF(e_instance);

    PyGILState_Release(gstate);

    return NULL;
}

static int _plugin_add_to_list(agent_plugin_t **list, PyObject *cls)
{
    agent_plugin_t *plugin;
    PyObject *mod_name;
    PyObject *cls_name;
    char *name;
    Py_ssize_t m_len;
    Py_ssize_t c_len;

    mod_name = PyObject_GetAttrString(cls, "__module__");
    cls_name = PyObject_GetAttrString(cls, "__name__");

    m_len = PyString_Size(mod_name);
    c_len = PyString_Size(cls_name);

    name = malloc(m_len + c_len + 2);
    if (name == NULL)
    {
        return -1;
    }

    snprintf(name, m_len + c_len + 2, "%s.%s",
            PyString_AsString(mod_name),
            PyString_AsString(cls_name));

    for(plugin = *list;plugin != NULL;plugin = plugin->next)
    {
        if (!strcmp(name, plugin->name))
        {
            Py_DECREF(plugin->cls);
            Py_INCREF(cls);
            plugin->cls = cls;
            return 0;
        }
    }
    
    plugin = calloc(1, sizeof(agent_plugin_t));
    if (plugin == NULL)
    {
        free(name);
        return -1;
    }

    Py_INCREF(cls);
    plugin->cls = cls;
    plugin->name = name;

    plugin->next = *list;
    *list = plugin;

    return 0;
}

static agent_plugin_t *_plugin_find(agent_plugin_t *list, PyObject *cls)
{
    agent_plugin_t *plugin;

    for(plugin = list;plugin != NULL;plugin = plugin->next)
    {
        if (plugin->cls == cls)
            break;
    }

    return plugin;
}

static int _exchange_plugin_init(PyObject *cls)
{
    PyObject *func;
    int err;

    func = PyObject_GetAttrString(cls, "get_request");
    if (func == NULL)
    {
        PyErr_Format(PyExc_AttributeError, "%s", "An 'exchange' plugin needs to define a 'get_request' method");
        return -1;
    }

#if 0
    if (!PyMethod_Check(func) && (Py_TYPE(func) != &PyMethodDescr_Type))
    {
        PyErr_Format(PyExc_TypeError, "%s", "'get_request' should be a method");
        return -1;
    }
#endif

    func = PyObject_GetAttrString(cls, "put_response");
    if (func == NULL)
    {
        PyErr_Format(PyExc_AttributeError, "%s", "An 'exchange' plugin needs to define a 'put_response' method");
        return -1;
    }

#if 0
    if (!PyMethod_Check(func) && !PyMethodDescr_Check(func))
    {
        PyErr_Format(PyExc_TypeError, "%s", "'put_response' should be a method");
        return -1;
    }
#endif

    err = _plugin_add_to_list(&_exchange_plugins, cls);
    if (err < 0)
    {
        return err;
    }

    return 0;
}

static int _parser_plugin_init(PyObject *cls)
{
    PyObject *func;
    int err;

    func = PyObject_GetAttrString(cls, "parse_request");
    if (func == NULL)
    {
        PyErr_Format(PyExc_AttributeError, "%s", "A 'parser' plugin needs to define a 'parse_request' method");
        return -1;
    }

    if (!PyMethod_Check(func))
    {
        PyErr_Format(PyExc_TypeError, "%s", "'parse_request' should be a method");
        return -1;
    }

    err = _plugin_add_to_list(&_parser_plugins, cls);
    if (err < 0)
    {
        return err;
    }

    return 0;
}

static int _metaclass_init(PyObject *cls, PyObject *args, PyObject *kwds)
{
    PyObject *cls_name;
    PyObject *cls_bases;
    PyObject *cls_attrs;
    PyObject *type_name;
    char *c_type_name;

    if (!PyArg_ParseTuple(args, "SO!O!",
                &cls_name,
                &PyTuple_Type, &cls_bases,
                &PyDict_Type, &cls_attrs))
    {
        PyErr_Format(PyExc_TypeError, "%s()' requires 3 arguments",
                PLUGIN_METATYPE_NAME);
        return -1;
    }

    /* Grab the type name for this plugin module */

    type_name = PyObject_GetAttrString(cls, "type");
    if ((type_name == NULL) || !PyString_Check(type_name))
    {
        PyErr_Format(PyExc_AttributeError,
                "Plugin '%s' needs to have a 'type' string attribute",
                PyString_AsString(cls_name));
        return -1;
    }

    c_type_name = PyString_AsString(type_name);

    if (!strcmp(c_type_name, "exchange"))
    {
        return _exchange_plugin_init(cls);
    }
    else if (!strcmp(c_type_name, "parser"))
    {
        return _parser_plugin_init(cls);
    }
    else
    {
        PyErr_Format(PyExc_AttributeError, "%s",
                "Plugin 'type' attribute should be "
                "'exchange' or 'parser'");
        return -1;
    }

    return 0;

}

static PyObject *_get_plugins(agent_plugin_t *plugin_list)
{
    agent_plugin_t *plugin;
    PyObject *plugins;

    plugins = PyList_New(0);
    if (plugins == NULL)
    {
        return NULL;
    }

    for(plugin = plugin_list;plugin != NULL;plugin = plugin->next)
    {
        /* 
         * Let's return them in the order they were loaded.  Our list
         * has them reversed... so we'll always Insert at the front
         * of the python list
         */
        PyList_Insert(plugins, 0, plugin->cls);
    }

    return plugins;
}

static PyObject *_get_exchange_plugins(PyObject *self, PyObject *args)
{
    return _get_plugins(_exchange_plugins);
}

static PyObject *_get_parser_plugins(PyObject *self, PyObject *args)
{
    return _get_plugins(_parser_plugins);
}

static PyObject *_na_module_set_parser(PyObject *self, PyObject *args, PyObject *kwargs)
{
    static char *keywords[] = { "exchanges", "parser", NULL };

    PyObject *exchanges = NULL;
    PyObject *parser = NULL;
    PyObject *exchange;
    Py_ssize_t list_sz = 0;
    agent_plugin_t *plugin;

    if (!PyArg_ParseTupleAndKeywords(args, kwargs, "O|O", keywords,
                &exchanges, &parser))
    {
        Py_RETURN_NONE;
    }

    if ((exchanges != NULL) &&
            !(PyType_Check(exchanges) || PyList_Check(exchanges)))
    {
        return PyErr_Format(PyExc_TypeError, "%s", "exchanges should be a single exchange class or a list of exchange classes");
    }

    if (!PyType_Check(parser))
    {
        return PyErr_Format(PyExc_TypeError, "%s", "parser is not a parser class");
    }

    if (_plugin_find(_parser_plugins, parser) == NULL)
    {
        return PyErr_Format(PyExc_TypeError, "%s", "parser is not a parser class");
    }

    if (PyList_Check(exchanges))
    {
        Py_ssize_t i = 0;

        list_sz = PyList_GET_SIZE(exchanges);
        if (list_sz == 0)
        {
            return PyErr_Format(PyExc_TypeError, "%s", "list of exchanges is empty");
            Py_RETURN_NONE;
        }

        for(;i < list_sz;i++)
        {
            exchange = PyList_GET_ITEM(exchanges, i);

            plugin = _plugin_find(_exchange_plugins, exchange);
            if (plugin == NULL)
            {
                return PyErr_Format(PyExc_TypeError, "%s", "Not all plugins in the 'exchanges' plugins list are 'exchange' plugins!");
            }

            Py_XDECREF(plugin->parser);
            Py_INCREF(parser);
            plugin->parser = parser;
        }
    }
    else
    {
        plugin = _plugin_find(_exchange_plugins, exchanges);
        if (plugin == NULL)
        {
            return PyErr_Format(PyExc_TypeError, "%s", "'exchanges' plugin argument is not really an 'exchange' plugin");
        }

        Py_XDECREF(plugin->parser);
        Py_INCREF(parser);
        plugin->parser = parser;
    }

    Py_RETURN_NONE;
}

static int _plugin_init(PyObject *cls, PyObject *args, PyObject *kwds)
{
    return 0;
}

static int _na_pymodule_init(void)
{
    PyObject *pymod;
    PyObject *main_mod;

    static PyMethodDef _na_module_methods[] =
    {
        { "exchange_plugins", (PyCFunction)_get_exchange_plugins,
                METH_NOARGS, "Get a list of loaded 'exchange' plugins" },
        { "parser_plugins", (PyCFunction)_get_parser_plugins,
                METH_NOARGS, "Get a list of loaded 'parser' plugins" },
        { "set_parser", (PyCFunction)_na_module_set_parser,
                METH_VARARGS|METH_KEYWORDS, "Set the parser class for an Exchange plugin or a list of exchange plugins" },
        { NULL, NULL, METH_NOARGS, NULL }
    };

    PyGILState_STATE gstate;

    /* Acquire GIL */
    gstate = PyGILState_Ensure();

    /* Create a new module */
    pymod = Py_InitModule(AGENT_MODULE_NAME, _na_module_methods);
    if (pymod == NULL)
    {
        /* Release GIL */
        PyGILState_Release(gstate);

        return -1;
    }

    main_mod = PyImport_AddModule("__main__");

    /* Add 'nova_agent' to the __main__ dictionary */
    PyObject_SetAttrString(main_mod, AGENT_MODULE_NAME, pymod);

    /* Inherit everything from 'type', and set our own init callback */
    agent_plugin_metatype.tp_base = &PyType_Type;
    agent_plugin_metatype.tp_flags = Py_TPFLAGS_DEFAULT;
    agent_plugin_metatype.tp_init = _metaclass_init;

    if (PyType_Ready(&agent_plugin_metatype) < 0)
    {
        return -1;
    }

    PyModule_AddObject(pymod, PLUGIN_METATYPE_NAME,
            (PyObject *)&agent_plugin_metatype);

    agent_plugin_class.tp_base = &PyBaseObject_Type;
    agent_plugin_class.tp_flags = Py_TPFLAGS_DEFAULT|Py_TPFLAGS_BASETYPE;
    agent_plugin_class.tp_new = PyType_GenericNew;
    agent_plugin_class.tp_init = _plugin_init;

    if (PyType_Ready(&agent_plugin_class) < 0)
    {
        return -1;
    }

    PyModule_AddObject(pymod, PLUGIN_CLASS_NAME,
            (PyObject *)&agent_plugin_class);

    /* Release GIL */
    PyGILState_Release(gstate);

    return 0;
}

int agent_plugin_init(agent_python_info_t *pi)
{
    int err;

    pthread_mutex_init(&_plugins_lock, NULL);

    err = _na_pymodule_init();
    if (err < 0)
    {
        pthread_mutex_destroy(&_plugins_lock);
        /* TODO: list_deinit()s */
        return err;
    }

    return 0;
}

void agent_plugin_deinit(void)
{
    pthread_mutex_destroy(&_plugins_lock);
    /* TODO: list_deinit()s */
}

int agent_plugin_start_exchanges(void)
{
    int err;
    int num_started = 0;
    agent_plugin_t *plugin;

    pthread_mutex_lock(&_plugins_lock);

    for(plugin = _exchange_plugins;plugin != NULL;plugin = plugin->next)
    {
        if (plugin->parser == NULL)
            continue;

        err = pthread_create(&(plugin->thr_id), NULL, _agent_parser_plugin_thread, plugin);
        if (err < 0)
        {
            agent_plugin_t *plugin2;

            /* TODO log */

            /* Leaving 'running' at 0 will cause threads to die when we unlock */

            pthread_mutex_unlock(&_plugins_lock);

            for(plugin2 = _exchange_plugins;plugin2 != plugin;
                    plugin2 = plugin2->next)
            {
                pthread_join(plugin2->thr_id, NULL);
            }

            return err;
        }

        num_started++;
    }

    if (num_started == 0)
    {
        pthread_mutex_unlock(&_plugins_lock);
        printf("No exchange plugins found with parsers set\n");
        return -1;
    }

    _plugins_running = 1;

    pthread_mutex_unlock(&_plugins_lock);

    return 0;
}

int agent_plugin_stop_exchanges(void)
{
    agent_plugin_t *plugin;

    pthread_mutex_lock(&_plugins_lock);

    _plugins_die = 1;

    for(plugin = _exchange_plugins;plugin != NULL;plugin = plugin->next)
    {
        pthread_join(plugin->thr_id, NULL);
    }

    pthread_mutex_unlock(&_plugins_lock);

    return 0;
}

int __attribute__ ((visibility("default"))) agent_plugin_register(const char *plugin_name, PyTypeObject *class_def, const char *type)
{
    PyGILState_STATE gstate;
    PyObject *cls_dict;
    int (*init_func)(PyObject *cls) = NULL;
    int err;
    PyObject *pymod;

    static PyMethodDef _mod_methods[] =
    {
        { NULL, NULL, METH_NOARGS, NULL }
    };

    if ((class_def == NULL) || (type == NULL))
    {
        return -1;
    }

    /* Acquire GIL */
    gstate = PyGILState_Ensure();

    cls_dict = PyDict_New();

    if (!strcmp(type, "exchange"))
    {
        init_func = _exchange_plugin_init;
    }
    else if (!strcmp(type, "parser"))
    {
        init_func = _parser_plugin_init;
    }
    else
    {
        fprintf(stderr, "Invalid plugin type, '%s'\n", type);
        Py_DECREF(cls_dict);
        /* Release GIL */
        PyGILState_Release(gstate); 
        return -1;
    }

    class_def->tp_base = &agent_plugin_class;
    PyDict_SetItemString(cls_dict, "type", PyString_FromString(type));
    class_def->tp_dict = cls_dict;

    pymod = Py_InitModule(plugin_name, _mod_methods);

    if (PyType_Ready(class_def) < 0)
    {
        Py_DECREF(cls_dict);

        /* Release GIL */
        PyGILState_Release(gstate); 
        return -1;
    }

    err = init_func((PyObject *)class_def);
    if (err < 0)
    {
        Py_DECREF(cls_dict);

        /* Release GIL */
        PyGILState_Release(gstate); 
        return -1;
    }

    PyModule_AddObject(pymod, plugin_name, (PyObject *)class_def);

    /* Release GIL */
    PyGILState_Release(gstate); 

    return err;
}

