
include $(top_srcdir)/Common.am

AM_CFLAGS += -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -DDATA_DIR=\"${datadir}\" -fvisibility=hidden

noinst_HEADERS = ../include/nova-agent_int.h ../include/python_int.h
include_HEADERS = ../include/nova-agent.h

sbin_PROGRAMS = nova-agent

check_PROGRAMS =

nova_agent_SOURCES = nova-agent.c python.c

nova_agent_LDADD = @PYTHON_LIB@ $(top_srcdir)/lib/libagent.la -lpthread
nova_agent_LDFLAGS = -export-dynamic
#-static

$(top_srcdir)/lib/libagent.la:
	cd $(top_srcdir)/lib && make
