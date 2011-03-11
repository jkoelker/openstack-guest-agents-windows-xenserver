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

#include "logging.h"


#define VSMPRINTF(p, fmt) \
    va_list ap;	\
    int n, size = 100;	\
    p = malloc(size);	\
    if (!p)	\
    {		\
        fprintf(stderr, "couldn't allocate %d bytes for buffer\n", size);	\
        return;	\
    }		\
    while (1)	\
    {		\
        va_start(ap, fmt);	\
        n = vsnprintf(p, size, fmt, ap);	\
        va_end(ap);		\
        if (n > -1 && n < size)	\
            break;		\
        if (n > -1)		\
            size = n + 1;	\
        else			\
            size *= 2;		\
        char *np = realloc(p, size);	\
        if (!np)		\
        {			\
            free(p);		\
            fprintf(stderr, "couldn't allocate %d bytes for buffer\n", size);	\
            return;		\
        }			\
        p = np;			\
    }


static void _log(char *level, char *p)
{
    static PyObject *logging = NULL;
    static int logging_imported = 0;

    if (!logging_imported)
    {
        logging = PyImport_ImportModule("logging");
        if (!logging)
            fprintf(stderr, "unable to import logging module\n");

        logging_imported = 1;
    }

    PyObject *ret = NULL;
    if (logging)
    {
        ret = PyObject_CallMethod(logging, level, "s", p);
        Py_XDECREF(ret);
    }

    if (!ret)
        fprintf(stderr, "%s\n", p);
}


void agent_debug(char *fmt, ...)
{
    char *p;
    VSMPRINTF(p, fmt);
    _log("debug", p);
    free(p);
}


void agent_error(char *fmt, ...)
{
    char *p;
    VSMPRINTF(p, fmt);
    _log("error", p);
    free(p);
}

