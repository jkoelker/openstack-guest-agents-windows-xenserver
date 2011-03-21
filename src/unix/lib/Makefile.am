
include $(top_srcdir)/Common.am

AM_CFLAGS += -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -DDATA_DIR=\"${datadir}\" -fvisibility=hidden

pkglib_LTLIBRARIES=agentlib.la

agentlib_la_SOURCES=agentlib.c plugin.c
agentlib_la_LDFLAGS = -module -avoid-version

nodist_noinst_DATA = agentlib.so

agentlib.so:
	    ln -s .libs/agentlib.so agentlib.so

clean-local:
	    rm -f agentlib.so


