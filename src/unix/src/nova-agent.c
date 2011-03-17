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
#include "logging.h"
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
                agent_error("got SIGINT\n");
                return NULL;

            case SIGTERM:
                agent_error("got SIGTERM\n");
                return NULL;

            case SIGCHLD:
                break;

            default:
                agent_debug("got sig %d\n", sig);
                continue;
        }
    }

    return NULL;
}

static void _usage(FILE *f, char *progname, int long_vers)
{
    fprintf(f, "Usage: %s [-h] [-n] [-o <filename>] [-l <level>] [-t] [--] config.py [optional arguments to config.py]\n", progname);
    if (long_vers)
    {
        fprintf(f, "\n");
        fprintf(f, "  config.py      Python configuration file to load\n\n");
        fprintf(f, "Options:\n");
        fprintf(f, "  -h, --help     Output this help information\n");
        fprintf(f, "  -n, --nofork   Don't fork into the background\n");
        fprintf(f, "  -o, --logfile  Call logging.basicConfig with filename\n");
        fprintf(f, "  -l, --level    Call logging.basicConfig with level\n");
        fprintf(f, "  -t, --testmode Treat self like 'python' binary\n");
        fprintf(f, "  -q, --quiet    Don't output startup/shutdown messages to stdout\n");
        fprintf(f, "  -S, --syspython Use system python (for building only)\n");
    }
    else
    {
        fprintf(f, "Try '%s --help' for more information.\n", progname);
    }
}

int main(int argc, char **argv)
{
    struct option longopts[] =
    {
        { "help", no_argument, NULL, 'h' },
        { "nofork", no_argument, NULL, 'n' },
        { "logfile", required_argument, NULL, 'o' },
        { "level", required_argument, NULL, 'l' },
        { "testmode", no_argument, NULL, 't' },
        { "quiet", no_argument, NULL, 'q' },
        { "syspython", no_argument, NULL, 'S' },
        { NULL, 0, NULL, 0 }
    };

    agent_python_info_t *pi;
    sigset_t mask;
    pthread_t thr_id;
    int opt;
    int err;
    int do_fork = 1;
    int test_mode = 0;
    int quiet = 0;
    int syspython = 0;
    char *progname = argv[0];
    char *logfile = NULL;
    char *level = NULL;
    char *config_file = NULL;

    /* Don't let getopt_long() output to stderr directly */
    opterr = 0;
    while((opt = getopt_long(argc, argv, ":hno:l:tqS", longopts, NULL)) != -1)
    {
        switch(opt)
        {
            case 'h':
                _usage(stdout, progname, 1);
                return 0;

            case 'n':
                do_fork = 0;
                return 0;

            case 'o':
                logfile = optarg;
                break;

            case 'l':
                level = optarg;
                break;

            case 't':
                test_mode = 1;
                break;

            case 'q':
                quiet = 1;
                break;

            case 'S':
                syspython = 1;
                break;

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

    if ((argc < 1) && !test_mode)
    {
        fprintf(stderr, "Error: No python configuration file specified\n");
        _usage(stderr, progname, 0);
        return 1;
    }

    config_file = argv[0];

    /*
     * Leave argc and argv alone.  We'll pass them into python and
     * it wants the script name as argv[0]...
     */

    if (!quiet)
        printf("Agent starting.\n");

    pi = agent_python_init(argc, argv, syspython);
    if (pi == NULL)
    {
        return 1;
    }

    err = agent_open_log(logfile, level);
    if (err < 0)
    {
        agent_python_deinit(pi);
        exit(-err);
    }

    /* init the plugin system */
    err = agent_plugin_init(pi);
    if (err < 0)
    {
        agent_error("couldn't init the plugin system: %d", err);
        agent_python_deinit(pi);
        exit(-err);
    }

    if (test_mode)
    {
        if (!argc)
        {
            err = agent_python_start_interpreter(pi);
        }
        else
        {
            err = agent_python_run_file(pi, config_file);
        }

        agent_python_deinit(pi);

        exit(err);
    }

    /* block signals */

    err = pthread_create(&thr_id, NULL, _signal_handler_thread, NULL);
    if (err)
    {
        agent_error("couldn't create signal handler thread: %s",
                strerror(err));
        exit(err);
    }

    sigfillset(&mask);
    sigdelset(&mask, SIGSEGV);
    sigdelset(&mask, SIGSTOP);

    pthread_sigmask(SIG_SETMASK, &mask, NULL);

    err = agent_python_run_file(pi, config_file);
    if (err < 0)
    {
        agent_error("failed to parse config file '%s'", argv[0]);
        agent_python_deinit(pi);
        exit(-err);
    }

    test_mode = agent_python_test_mode(pi);
    if (test_mode < 0)
    {
        /* Switch this to the new python error call when logging-dev is
         * merged
         */
        agent_error("test_mode error");
        agent_python_deinit(pi);
        exit(1);
    }

    if (test_mode)
    {
        /* Test mode */

        if (!quiet)
            printf("Agent stopping due to test mode.\n");

        agent_python_deinit(pi);
        exit(0);
    }

    /* Continue */

    err = agent_plugin_start_exchanges();
    if (err < 0)
    {
        agent_error("failed to start exchange plugins");
        agent_python_deinit(pi);
        exit(-err);
    }

    if (!quiet)
        agent_info("Agent started");

    pthread_join(thr_id, NULL);

    if (!quiet)
        agent_info("Agent stopping");

    agent_plugin_stop_exchanges();

    agent_plugin_deinit();
    agent_python_deinit(pi);

    if (!quiet)
        printf("Agent stopping.\n");

    return 0;
}
