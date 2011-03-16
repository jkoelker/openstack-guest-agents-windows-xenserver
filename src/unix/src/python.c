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
#include <fcntl.h>
#include <sys/types.h>
#include <string.h>
#include <errno.h>
#include "python.h"
#include "logging.h"
#include "plugin_int.h"

struct _agent_python_info
{
    PyThreadState *main_thread_state;
    PyObject *main_dict;
};


static PyObject *_agent_python_get_code(const char *filename)
{
    int fd;
    int err;
    char *python_code;
    struct stat sb;
    PyObject *code_obj;

    fd = open(filename, O_RDONLY);
    if (fd < 0)
    {
        agent_error("Couldn't open '%s'\n", filename);
        return NULL;
    }

    err = fstat(fd, &sb);
    if (err < 0)
    {
        agent_error("Couldn't fstat '%s'\n", filename);
        close(fd);
        return NULL;
    }

    python_code = malloc(sb.st_size + 1);
    if (python_code == NULL)
    {
        agent_error("Out of memory allocating space for '%s'\n", filename);
        close(fd);
        return NULL;
    }

    err = read(fd, python_code, sb.st_size);
    if (err != sb.st_size)
    {
        agent_error("Error reading from '%s'\n", filename);
        close(fd);
        free(python_code);
        return NULL;
    }

    close(fd);

    /* Terminate the data we read */
    python_code[sb.st_size] = '\0';

    code_obj = Py_CompileString(python_code, filename, Py_file_input);

    free(python_code);

    return code_obj;
}

/* Assumes GIL is acquired */
static PyObject *_agent_python_run_file(const char *filename, PyObject *dict)
{
    PyObject *code_obj;
    PyObject *result;

    code_obj = _agent_python_get_code(filename);
    if (code_obj == NULL)
    {
        if (PyErr_Occurred())
        {
            agent_python_handle_error("Failed to compile python code");
        }
        return NULL;
    }

    result = PyEval_EvalCode((PyCodeObject *)code_obj, dict, dict);

    /* Don't with the code object */
    Py_DECREF(code_obj);

    if (PyErr_Occurred() != NULL)
    { 
        PyObject *ptype = NULL;
        PyObject *pvalue = NULL;
        PyObject *ptraceback = NULL;

        PyErr_Fetch(&ptype, &pvalue, &ptraceback);

        if (!PyErr_GivenExceptionMatches(ptype, PyExc_SystemExit))
        { 
            PyErr_Restore(ptype, pvalue, ptraceback);

            agent_python_handle_error("Failed to run python code");
        }
        else
        {
            Py_XDECREF(ptype);
            Py_XDECREF(pvalue);
            Py_XDECREF(ptraceback);
        }
    }

    return result;
}

#if 0
static PyObject *_agent_python_load_module(const char *filename, const char *mod_name)
{
    PyObject *code_obj;
    PyObject *result;

    code_obj = _agent_python_get_code(filename);
    if (code_obj == NULL)
    {
        return NULL;
    }

    result = PyImport_ExecCodeModule((char *)mod_name, code_obj);

    Py_DECREF(code_obj);

    if (PyErr_Occurred() != NULL)
    { 
        PyObject *ptype = NULL;
        PyObject *pvalue = NULL;
        PyObject *traceback = NULL;

        PyErr_Print();
        PyErr_Fetch(&ptype, &pvalue, &traceback);

        if (!PyErr_GivenExceptionMatches(ptype, PyExc_SystemExit))
        { 
            agent_error("Failed to run Python code");

            PyErr_Clear();

            Py_XDECREF(result);
            result = NULL;
        }

        if (ptype != NULL)
        {
            Py_DECREF(ptype);
        }

        if (pvalue != NULL)
        {
            Py_DECREF(pvalue);
        }

        if (traceback != NULL)
        {
            Py_DECREF(traceback);
        }
    }

    return result;
}
#endif

agent_python_info_t *agent_python_init(void)
{
    agent_python_info_t *pi;
    PyObject *main_module;

    Py_Initialize();
    PyEval_InitThreads();

    main_module = PyImport_AddModule("__main__");
    if (main_module == NULL)
    {
        Py_Finalize();
        return NULL;
    }

    /* Note: main_module is a borrowed reference, so we won't need to
     * DECREF it
     */
    
    pi = calloc(1, sizeof(agent_python_info_t));
    if (pi == NULL)
    {
        Py_Finalize();
        return NULL;
    }

    pi->main_dict = PyModule_GetDict(main_module);

//    PySys_SetPath("./lib");

    /* Swap out and return current thread state and release the GIL */
    pi->main_thread_state = PyEval_SaveThread();

    return pi;
}

void agent_python_deinit(agent_python_info_t *pi)
{
    /* Acquire the GIL before shutting down */
    PyEval_RestoreThread(pi->main_thread_state);

    Py_Finalize();
    free(pi);
}


int agent_python_run_file(agent_python_info_t *pi, const char *filename)
{
    PyGILState_STATE gstate;
    PyObject *result;
    int err;

    /* Acquire GIL */
    gstate = PyGILState_Ensure();

    result = _agent_python_run_file(filename, pi->main_dict);

    if (result == NULL)
        err = -1;
    else
        err = 0;

    Py_XDECREF(result);

    /* Release GIL */
    PyGILState_Release(gstate); 

    return err;
}

/* Assumes GIL is acquired */
PyObject *agent_python_dict_create(PyMethodDef *methods, PyObject *self)
{
    PyMethodDef *def;
    PyObject *dict;
    PyObject *c_func;
    PyObject *c_method;

    dict = PyDict_New();

    if (methods == NULL)
        return dict;

    for(def = methods;def->ml_name != NULL;def++)
    {
        c_func = PyCFunction_New(def, self);
        c_method = PyMethod_New(c_func, NULL, NULL);
        Py_DECREF(c_func);

        PyDict_SetItemString(dict, def->ml_name, c_method);

        Py_DECREF(c_method);
    }

    return dict;
}

/* Assumes GIL is acquired */
PyObject *agent_python_class_create(PyObject *module, const char *name,
        PyObject *meta_cls, PyObject *base_cls, PyObject *cls_dict,
        int set_metaclass)
{
    PyObject *bases;
    PyObject *cls;
    PyObject *cls_name;

    cls_name = PyString_FromString(name);

    if (base_cls != NULL)
    {
        /* bases needs to be a tuple, but we're only receiving 1 base */
        bases = PyTuple_New(1);
        PyTuple_SetItem(bases, 0, base_cls);
    }
    else
    {
        bases = PyTuple_New(0);
    }

    /*
     * We create a new class by calling type('name', bases, cls_dict)
     *
     * 'type' could be a subclass of 'type', which can be used to
     * set a metaclass for the new class.  An __init__ call in such
     * a metaclass can be used to catch all subclassing...
     *
     */

    Py_INCREF(meta_cls);

    cls = PyObject_CallFunctionObjArgs(meta_cls, cls_name,
            bases, cls_dict, NULL);
    if (cls == NULL)
    {
        Py_DECREF(meta_cls);
        Py_DECREF(cls_name);
        Py_DECREF(bases);

        return NULL;
    }

    Py_DECREF(meta_cls);

    if (set_metaclass)
    {
        /* 
         * Python v2 syntax for defining a class's metaclass is like so:
         *
         * class MyClass(object):
         *     __metaclass__ = module_meta_class
         *
         * So, we'll go ahead and set this attribute if requested.
         *
         * It doesn't appear to be needed when using the C API, because
         * we always create classes from a metaclass of some sort.
         *
         * We'll still set it, just to be consistent..
         */
        PyObject_SetAttrString(cls, "__metaclass__", meta_cls);
    }

    /* Add the class name to the module's dictionary */
    PyObject_SetAttrString(module, name, cls);

    Py_DECREF(cls_name);
    Py_DECREF(bases);

    return cls;
}

int agent_python_handle_error(char *log_prefix)
{
    PyObject *ptype;
    PyObject *pvalue;
    PyObject *ptraceback;

    /* Acquire GIL */
    PyGILState_STATE gstate = PyGILState_Ensure();

    PyErr_Fetch(&ptype, &pvalue, &ptraceback);

    if (ptype == NULL)
    {
        agent_error("%s: No python error available", log_prefix);

        /* Release GIL */
        PyGILState_Release(gstate); 
        return -1;
    }

    PyObject *tb_mod = PyImport_AddModule("traceback");
    if (tb_mod == NULL)
    {
        PyErr_Clear();

        Py_XDECREF(ptype);
        Py_XDECREF(pvalue);
        Py_XDECREF(ptraceback);

        agent_error("%s: [Couldn't find traceback module "
                "to print the error]", log_prefix);

        /* Release GIL */
        PyGILState_Release(gstate); 
        return -1;
    }

    PyObject *pobj_list = PyObject_CallMethod(tb_mod, "format_exception",
            "OOO", ptype, pvalue, ptraceback);
    if (pobj_list == NULL)
    {
        PyErr_Clear();

        Py_XDECREF(ptype);
        Py_XDECREF(pvalue);
        Py_XDECREF(ptraceback);

        agent_error("%s: [Couldn't format traceback]", log_prefix);

        /* Release GIL */
        PyGILState_Release(gstate); 
        return -1;
    }

    Py_XDECREF(ptype);
    Py_XDECREF(pvalue);
    Py_XDECREF(ptraceback);

    agent_error("%s: A python exception has occurred:", log_prefix);

    Py_ssize_t list_sz = PyList_Size(pobj_list);
    PyObject *pobj_str;

    Py_ssize_t i;

    for(i = 0;i < list_sz;i++)
    {
        pobj_str = PyList_GetItem(pobj_list, i);

        char *obj_str = strdup(PyString_AsString(pobj_str));

        Py_DECREF(pobj_str);
    
        if (obj_str == NULL)
        {
            agent_error("Out of memory");

            Py_DECREF(pobj_list);
    
            /* Release GIL */
            PyGILState_Release(gstate); 
            return 0;
        }
    
        char *ptr = strchr(obj_str, '\n');
        if (ptr == NULL)
        {
            agent_error("[EXC] %s", obj_str);
            free(obj_str);

            continue;
        }

        char *start = obj_str;
        *(ptr++) = '\0';

        agent_error("[EXC] %s", start);
    
        while((ptr != NULL) && (*ptr != '\0'))
        {
            start = ptr;
            ptr = strchr(start, '\n');
            if (ptr != NULL)
            {
                *ptr++ = '\0';
            }

            agent_error("[EXC] %s", start);
        }
    
        free(obj_str);
    }

    /* Release GIL */
    PyGILState_Release(gstate); 

    return 0;
}
