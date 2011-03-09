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

#ifndef __NOVA_AGENT_PLUGIN_H__
#define __NOVA_AGENT_PLUGIN_H__

#include <sys/types.h>
/* Stupid hack.  Python.h redefines this */
#undef _POSIX_C_SOURCE
#include <Python.h>

typedef struct _agent_plugin_def agent_plugin_def_t;

struct _agent_plugin_def
{
#define NOVA_AGENT_PLUGIN_VERSION 1
    unsigned int version;

    PyObject *(*init_callback)(PyObject *self, PyObject *args);
    PyObject *(*deinit_callback)(PyObject *self, PyObject *args);

    union
    {
        struct /* _agent_exchange_plugin */
        {
            PyObject *(*get_request)(PyObject *self, PyObject *args);
            PyObject *(*put_response)(PyObject *self, PyObject *args);
        };

        struct /* _agent_parser_plugin */
        {
            PyObject *(*parse_request)(PyObject *self, PyObject *args);
        };
    };
};

#if 0
int agent_plugin_register(const char *mod_name, const char *type, agent_plugin_def_t *mod_def);
#endif

int agent_plugin_register(const char *plugin_name, PyTypeObject *class_def, const char *type);

#endif /* __NOVA_AGENT_PLUGIN_H__ */
