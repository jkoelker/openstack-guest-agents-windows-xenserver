
pluginsdir=$(datadir)/nova-agent/plugins

AM_CFLAGS =-I../../include -I/usr/include/python2.6 -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -Wall -Werror $(GCOV_CFLAGS)

pkglib_LTLIBRARIES=xenstore.la

xenstore_la_SOURCES=xenstore.c
xenstore_la_LIBADD = -lxenstore
xenstore_la_LDFLAGS = -module -avoid-version

