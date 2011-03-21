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
#include <sys/types.h>
#include <assert.h>
#include <xs.h>
#include "plugin.h"
#include "logging.h"

#define XENSTORE_MODULE_NAME "xenstore"
#define XENSTORE_CLASS_NAME "xenstore"


#define XENSTORE_REQUEST_PATH "data/host"
#define XENSTORE_RESPONSE_PATH "data/guest"


typedef struct _xenstore_info xenstore_info_t;

struct _xenstore_info
{
    PyObject_HEAD
    struct xs_handle *handle;
    PyObject *requests;
};

static PyTypeObject _xenstore_type =
{
    PyVarObject_HEAD_INIT(NULL, 0)
    "xenserver.na_xenstore", /*tp_name*/
    sizeof(xenstore_info_t),/*tp_basicsize*/
    0,/*tp_itemsize*/
};


#define NETWORKRESET_EVENT "{\"name\":\"resetnetwork\",\"value\":"
#define XENSTORE_NETWORKING_PATH "vm-data/networking"
#define XENSTORE_HOSTNAME_PATH "vm-data/hostname"


static int _string_append(char **dest_string, u_int *dest_string_len,
        u_int *dest_string_sz, char *src_string, u_int src_string_len)
{
    u_int sz_needed;

    sz_needed = *dest_string_len + src_string_len + 32;

    if (sz_needed > *dest_string_sz)
    {
        void *ptr = realloc(*dest_string, sz_needed);

        if (!ptr)
        {
            return -1;
        }

        *dest_string = ptr;
        *dest_string_sz = sz_needed;
    }

    memcpy(*dest_string + *dest_string_len, src_string, src_string_len);

    (*dest_string_len) += src_string_len;

    return 0;
}

/*
 * Pull the networking data out of the xenstore, and return a new
 * buffer that contains 'prefix' appended with the networking data
 * Prefix is assumed to be of format:
 *    "{\"opt\":\"opt_value\",\"value\":"
 * The final '}' will be appended by this call.
 *
 * As for 'value', it will be a dictionary containing these keys:
 * 'hostname' -- The value is hostname string or ""
 * 'interfaces' -- This will be a list of interfaces.
 */
static int _convert_network_event(xenstore_info_t *xsi, xs_transaction_t t, char *prefix, char **buf, unsigned int *buflen)
{
    char dir_entry_path[255];
    char **dir_list;
    unsigned int dir_list_num;
    void *dir_entry;
    unsigned int dir_entry_len;
    void *hostname;
    unsigned int hostname_len;
    char **d;
    unsigned int i;
    char *new_buf = NULL;
    unsigned int new_buf_len = 0;
    unsigned int new_buf_sz = 0;
    static char prefix2[] = "{\"hostname\":\"";
    static char prefix3[] = "\",\"interfaces\":[";

    hostname = xs_read(xsi->handle, t, XENSTORE_HOSTNAME_PATH,
                        &hostname_len);
    if (hostname == NULL)
    {
        hostname = strdup("");
        if (hostname == NULL)
        {
            return -1;
        }

        hostname_len = 0;
    }

    dir_list = xs_directory(xsi->handle, t,
                                XENSTORE_NETWORKING_PATH, &dir_list_num);
    if ((dir_list == NULL) ||
            (dir_list_num == 0))
    {
        agent_debug("No interface entries found in xenstore path '%s'",
                XENSTORE_NETWORKING_PATH);

        dir_list_num = 0; /* make sure, if dir_list == NULL */
        /* fall through */
    }

#define STRING_APPEND(__x, __x_len) \
        _string_append(&new_buf, &new_buf_len, &new_buf_sz, \
                __x, __x_len)

    /* 
     * We're going to add starting:
     * {"hostname": hostname, "interfaces": [
     *
     * And we'll need to add ending ']' and '}'
     *
     */

    if (STRING_APPEND(prefix, strlen(prefix)) < 0)
    {
        free(hostname);
        return -1;
    }

    if (STRING_APPEND(prefix2, strlen(prefix2)) < 0)
    {
        free(hostname);
        return -1;
    }

    if (STRING_APPEND(hostname, hostname_len) < 0)
    {
        free(hostname);
        free(new_buf);
        return -1;
    }

    /* Don't need this anymore */
    free(hostname);

    if (STRING_APPEND(prefix3, strlen(prefix3)) < 0)
    {
        free(new_buf);
        return -1;
    }

    /* Won't execute if dir_list is NULL */
    for(i=0,d=dir_list;i<dir_list_num;i++,d++)
    {
        snprintf(dir_entry_path, sizeof(dir_entry_path), "%s/%s",
                XENSTORE_NETWORKING_PATH, *d);

        agent_debug("Reading xenstore interface entry '%s'", dir_entry_path);

        dir_entry = xs_read(xsi->handle, t, dir_entry_path, &dir_entry_len);
        if (dir_entry == NULL)
        {
            free(new_buf);
            free(dir_list);
            return -1;
        }

        if (i != 0)
        {
            /* 
             * Always room for 1 character, so we don't need to check
             * for errors
             */
            STRING_APPEND(",", 1);
        }

        if (STRING_APPEND(dir_entry, dir_entry_len) < 0)
        {
            free(dir_entry);
            free(new_buf);
            free(dir_list);
            return -1;
        }

        free(dir_entry);
    }

    /*
     * Always room for 4 characters, so we don't need to check for
     * errors.  I say 4, because we're going to set \0 too
     *
     */
    STRING_APPEND("]}}", 3);
    new_buf[new_buf_len] = '\0';

    free(dir_list);

    *buf = new_buf;
    *buflen = new_buf_len;

    return 0;
}

static int _fill_requests(xenstore_info_t *xsi, char *path)
{
    char entry_path[255];
    char **entries;
    char **entry;
    char *entry_data;
    unsigned int num_entries;
    unsigned int entry_data_len;
    xs_transaction_t t;
    PyObject *obj;

    t = xs_transaction_start(xsi->handle);
    if (t == XBT_NULL)
    {
        return -1;
    }

    entries = xs_directory(xsi->handle, t, path, &num_entries);
    if (entries == NULL)
    {   
        if (!xs_transaction_end(xsi->handle, t, 0))
        {   
            /* Connection could be closed and needs reopened */
            return -1;
        }

        return 0;
    }

    for(entry=entries;num_entries > 0;--num_entries, entry++)
    {
        snprintf(entry_path, sizeof(entry_path), "%s/%s",
                path, *entry);

        entry_data = xs_read(xsi->handle, t, entry_path, &entry_data_len);
        if (entry_data == NULL)
        {
            free(entries);
            /* Connection could be closed and needs reopened */
            return -1;
        }

        agent_debug("Reading xenstore entry '%s'", entry_path);

        /* We read from the path, now do something with the data */

        if (!strncmp(entry_data, NETWORKRESET_EVENT, strlen(NETWORKRESET_EVENT)))
        {
            /*
             * A 'networkreset' event comes in with an empty value, so we
             * need to pull the networking data out of a separate path
             * in xenstore and shove it in as the value
             */

            free(entry_data);

            if (_convert_network_event(xsi, t, NETWORKRESET_EVENT,
                    &entry_data, &entry_data_len) < 0)
            {
                free(entries);
                /* Connection could be closed and needs reopened */
                return -1;
            }
        }

        obj = PyDict_New();
        PyDict_SetItemString(obj, "path", PyString_FromString(entry_path));
        PyDict_SetItemString(obj, "data",
                PyString_FromStringAndSize(entry_data, entry_data_len));

        PyList_Append(xsi->requests, obj);

        Py_DECREF(obj);

        free(entry_data);
    }

    if (!xs_transaction_end(xsi->handle, t, 0))
    {
        free(entries);
        /* Connection could be closed and needs reopened */
        return -1;
    }

    free(entries);

    return 0;
}

static int _xenstore_init(xenstore_info_t *xsi, PyObject *args,
        PyObject *kwargs)
{
    int err;

    /* Call our base for initing */
    err = Py_TYPE(xsi)->tp_base->tp_init((PyObject *)xsi, args, kwargs);
    if (err < 0)
    {
        return err;
    }

    xsi->handle = xs_domain_open();
    if (xsi->handle == NULL)
    {
        PyErr_Format(PyExc_SystemError, "%s", "Couldn't open xenstore");
        return -1;
    }

    xsi->requests = PyList_New(0);

    return 0;
}

static void _xenstore_del(xenstore_info_t *xsi)
{
    if (xsi->handle != NULL)
        xs_daemon_close(xsi->handle);
    Py_XDECREF(xsi->requests);
}

static PyObject *_xenstore_get_request(xenstore_info_t *xsi, PyObject *args)
{
    static PyObject *pop_name = NULL;
    static PyObject *zero_obj = NULL;
    int err;

    if (PyTuple_Size(args) != 0)
    {
        return PyErr_Format(PyExc_SystemError, "%s", "No arguments expected to xenstore.get_request()");
    }

    if (pop_name == NULL)
    {
        pop_name = PyString_FromString("pop");
        zero_obj = PyInt_FromLong(0);
    }

    if (PyList_GET_SIZE(xsi->requests) == 0)
    {
        err = _fill_requests(xsi, XENSTORE_REQUEST_PATH);
        if (err < 0)
        {
            Py_RETURN_NONE;
        }

        if (PyList_GET_SIZE(xsi->requests) == 0)
        {
            Py_RETURN_NONE;
        }
    }

    return PyObject_CallMethodObjArgs(xsi->requests, pop_name, zero_obj, NULL);
}

static PyObject *_xenstore_put_response(xenstore_info_t *xsi,
        PyObject *args)
{
    PyObject *req;
    PyObject *resp;
    PyObject *req_path;
    PyObject *resp_data;
    char *path_to_rm;
    char path_to_write[255];
    size_t req_path_len;

    if (!PyArg_ParseTuple(args, "OO", &req, &resp))
    {
        return PyErr_Format(PyExc_SystemError, "%s", "Invalid number of arguments to xenstore.put_response()");
    }

    if (!PyDict_Check(req))
    {
        Py_RETURN_NONE;
    }

    req_path = PyDict_GetItemString(req, "path");
    if (!req_path)
    {
        return PyErr_Format(PyExc_SystemError, "%s", "Request has no 'path' key when trying to send a response");
    }

    if (!PyString_Check(req_path))
    {
        return PyErr_Format(PyExc_SystemError, "%s", "Request's 'path' value is not a string");
    }

    path_to_rm = PyString_AsString(req_path);
    req_path_len = strlen(XENSTORE_REQUEST_PATH);

    if (strncmp(path_to_rm, XENSTORE_REQUEST_PATH, req_path_len) ||
            path_to_rm[req_path_len] != '/')
    {
        return PyErr_Format(PyExc_SystemError, "%s", "Request's 'path' value is has been modified");
    }

    path_to_rm = PyString_AsString(req_path);

    if (!xs_rm(xsi->handle, XBT_NULL, path_to_rm))
    {
        agent_warn("Removing xenstore entry '%s' failed", path_to_rm);
    }

    if (resp == Py_None)
    {
        agent_warn("Ignoring empty response in _xenstore_put_response");
        Py_RETURN_NONE;
    }

    if (!PyDict_Check(resp))
    {
        return PyErr_Format(PyExc_SystemError, "%s", "Response is expected to be a dictionary");
    }

    resp_data = PyDict_GetItemString(resp, "data");
    if (!resp_data)
    {
        return PyErr_Format(PyExc_SystemError, "%s", "Response has no 'data' key when trying to send a response");
    }

    if (!PyString_Check(resp_data))
    {
        return PyErr_Format(PyExc_SystemError, "%s", "Response's 'data' value should be a string");
    }

    snprintf(path_to_write, sizeof(path_to_write), "%s%s",
            XENSTORE_RESPONSE_PATH, path_to_rm + req_path_len);

    if (!xs_write(xsi->handle, XBT_NULL, path_to_write,
                        PyString_AsString(resp_data),
                        PyString_GET_SIZE(resp_data)))
    {
        agent_error("Failed to write xenstore entry to '%s'",
                path_to_write);
    }

    Py_RETURN_NONE;
}

static PyMethodDef _xenstore_methods[] =
{
    { "get_request", (PyCFunction)_xenstore_get_request, METH_VARARGS,
            "xenstore plugin method that returns a new request" },
    { "put_response", (PyCFunction)_xenstore_put_response, METH_VARARGS,
            "xenstore plugin method that puts a response" },
    { NULL, NULL, 0, NULL }
};

PyMODINIT_FUNC initxenstore(void)
{
    static PyMethodDef _mod_methods[] =
    {   
        { NULL, NULL, METH_NOARGS, NULL }
    };

    _xenstore_type.tp_alloc = PyType_GenericAlloc;
    _xenstore_type.tp_new = PyType_GenericNew;
    _xenstore_type.tp_methods = _xenstore_methods;
    _xenstore_type.tp_init = (initproc)_xenstore_init;
    _xenstore_type.tp_flags = Py_TPFLAGS_DEFAULT|Py_TPFLAGS_BASETYPE;
    _xenstore_type.tp_del = (destructor)_xenstore_del;

    PyObject *pymod = Py_InitModule(XENSTORE_MODULE_NAME, _mod_methods);

    if (PyType_Ready(&_xenstore_type) < 0)
    {  
        PyErr_Format(PyExc_SystemError, "Couldn't init xenstore class");
        return;
    }

    PyModule_AddObject(pymod, XENSTORE_CLASS_NAME,
            (PyObject *)&_xenstore_type);

}

