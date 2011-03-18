
pluginsdir=$(datadir)/nova-agent/plugins

#AM_CFLAGS =-I../include @PYTHON_CFLAGS@ -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -Wall -Werror $(GCOV_CFLAGS) -fvisibility=hidden
AM_CFLAGS =-I../include @PYTHON_CFLAGS@ -Wall -Werror $(GCOV_CFLAGS)
AM_CFLAGS +=-DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -fvisibility=hidden

sbin_PROGRAMS = nova-agent

check_PROGRAMS =

nova_agent_SOURCES = nova-agent.c python.c plugin.c logging.c

nova_agent_LDADD = @PYTHON_LIB@ -lpthread
nova_agent_LDFLAGS = -export-dynamic
#-static

