
include $(top_srcdir)/Common.am

AM_CFLAGS += -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -fvisibility=hidden

sbin_PROGRAMS = nova-agent

check_PROGRAMS =

nova_agent_SOURCES = nova-agent.c python.c plugin.c logging.c

nova_agent_LDADD = @PYTHON_LIB@ -lpthread
nova_agent_LDFLAGS = -export-dynamic
#-static

