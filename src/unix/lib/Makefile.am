
include $(top_srcdir)/Common.am

AM_CFLAGS += -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -DDATA_DIR=\"${datadir}\" -fvisibility=hidden

noinst_HEADERS = agentlib_int.h plugin_int.h logging_int.h
include_HEADERS = agentlib.h plugin.h logging.h

pkglib_LTLIBRARIES = libagentlib.la

libagentlib_la_SOURCES = agentlib.c plugin.c logging.c
libagentlib_la_LDFLAGS = -avoid-version

nodist_noinst_DATA = agentlib.so

agentlib.so:
	ln -s .libs/libagentlib.so agentlib.so

clean-local:
	    rm -f agentlib.so


