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
#include <sys/wait.h>
#include <getopt.h>
#include <string.h>
#include <signal.h>
#include <errno.h>
#include "nova-agent.h"
#include "python.h"
#include "plugin_int.h"

static void *_signal_handler_thread(void *args)
{
    sigset_t mask;
    int err;
    int sig;

    sigfillset(&mask);
    sigdelset(&mask, SIGSEGV);
    sigdelset(&mask, SIGSTOP);
    pthread_sigmask(SIG_SETMASK, &mask, NULL);

    for(;;)
    {
        err = sigwait(&mask, &sig);
        if (err < 0)
            continue;

        switch(sig)
        {
            case SIGINT:
                printf("Got SIGINT\n");
                return NULL;

            case SIGTERM:
                printf("Got SIGTERM\n");
                return NULL;

            case SIGCHLD:
                break;

            default:
                printf("Got sig %d\n", sig);
                continue;
        }
    }

    return NULL;
}

static void _usage(FILE *f, char *progname, int long_vers)
{
    fprintf(f, "Usage: %s [-h] config.py\n", progname);
    if (long_vers)
    {
        fprintf(f, "\n");
        fprintf(f, "  config.py      Python configuration file to load\n\n");
        fprintf(f, "Options:\n");
        fprintf(f, "  -h, --help     Output this help information\n");
        fprintf(f, "  -n, --nofork   Don't fork into the background\n");
    }
    else
    {
        fprintf(f, "Try '%s --help' for more information.\n", progname);
    }
}

int main(int argc, char * const *argv)
{
    struct option longopts[] =
    {
        { "help", no_argument, NULL, 'h' },
        { "nofork", no_argument, NULL, 'n' },
        { NULL, 0, NULL, 0 }
    };

    agent_python_info_t *pi;
    sigset_t mask;
    pthread_t thr_id;
    int opt;
    int err;
    int do_fork = 1;
    char *progname = argv[0];

    /* Don't let getopt_long() output to stderr directly */
    opterr = 0;
    while((opt = getopt_long(argc, argv, ":hn", longopts, NULL)) != -1)
    {
        switch(opt)
        {
            case 'h':
                _usage(stdout, progname, 1);
                return 0;

            case 'n':
                do_fork = 0;
                return 0;

            case ':':
                fprintf(stderr, "Error: Missing argument to option '%c'\n",
                    optopt);
                _usage(stderr, progname, 0);
                return 1;

            case '?':
                switch(optopt)
                {
                    case 'h':
                    case 'n':
                        fprintf(stderr,
                            "Error: Unexpected argument to option '%c'\n",
                            optopt);
                        break;

                    default:
                        fprintf(stderr, "Error: Unknown option: '%c'\n",
                            optopt);
                        break;
                }

                _usage(stderr, progname, 0);
                return 1;

            case 0:
                /* Reserved for options where 'flag != NULL' in longopts[] */
                break;

            default:
                fprintf(stderr, "Error: Unknown error: '%c'\n", opt);
                _usage(stderr, progname, 0);
                return 1;
        }
    }

    argv += optind;
    argc -= optind;

    if (argc != 1)
    {
        fprintf(stderr, "Error: No python configuration file specified\n");
        _usage(stderr, progname, 0);
        return 1;
    }

    printf("Agent starting.\n");

    pi = agent_python_init();
    if (pi == NULL)
    {
        return 1;
    }

    /* init the plugin system */
    err = agent_plugin_init(pi);
    if (err < 0)
    {
        fprintf(stderr, "Error: Couldn't init the plugin system: %d\n", err);
        agent_python_deinit(pi);
        exit(-err);
    }

    /* block signals */

    err = pthread_create(&thr_id, NULL, _signal_handler_thread, NULL);
    if (err)
    {
        fprintf(stderr, "Error: Couldn't create signal handler thread: %s",
                strerror(err));
        exit(err);
    }

    sigfillset(&mask);
    sigdelset(&mask, SIGSEGV);
    sigdelset(&mask, SIGSTOP);

    pthread_sigmask(SIG_SETMASK, &mask, NULL);

    err = agent_python_run_file(pi, argv[0]);
    if (err < 0)
    {
        fprintf(stderr, "Error parsing the python config file\n");
        agent_python_deinit(pi);
        exit(-err);
    }

    /* Continue */

    err = agent_plugin_start_exchanges();
    if (err < 0)
    {
        fprintf(stderr, "Error starting exchange plugins\n");
        agent_python_deinit(pi);
        exit(-err);
    }

    pthread_join(thr_id, NULL);

    agent_plugin_stop_exchanges();

    agent_plugin_deinit();
    agent_python_deinit(pi);

    printf("Agent stopping.\n");

    return 0;
}
