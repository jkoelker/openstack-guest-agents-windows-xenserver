
include $(top_srcdir)/Common.am

AM_CFLAGS += -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -DDATA_DIR=\"${datadir}\" -fvisibility=hidden

noinst_HEADERS = agentlib_int.h plugin_int.h logging_int.h
include_HEADERS = agentlib.h plugin.h logging.h

lib_LTLIBRARIES = libagent.la

libagent_la_SOURCES = agentlib.c plugin.c logging.c
libagent_la_LDFLAGS = -avoid-version

nodist_noinst_DATA = libagent.so

libagent.so:
	ln -s .libs/libagent.so libagent.so

clean-local:
	    rm -f libagent.so


