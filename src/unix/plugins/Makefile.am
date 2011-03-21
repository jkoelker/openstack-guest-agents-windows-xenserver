
include $(top_srcdir)/Common.am

dist_plugins_SCRIPTS = __init__.py

nobase_dist_plugins_SCRIPTS = xenstore/__init__.py \
							  jsonparser/__init__.py \
							  jsonparser/jsonparser.py

nobase_pkglib_LTLIBRARIES=xenstore/xenstore.la

xenstore_xenstore_la_SOURCES=xenstore/xenstore.c
xenstore_xenstore_la_LIBADD = -lxenstore
xenstore_xenstore_la_LDFLAGS = -module -avoid-version

nodist_noinst_DATA = xenstore/xenstore.so

xenstore/xenstore.so:
	ln -s .libs/xenstore.so xenstore/xenstore.so

clean-local:
	rm -f xenstore/xenstore.so

install-exec-local:
	@$(top_srcdir)/src/nova-agent -qS -- $(top_srcdir)/install_libs.py xenstore/xenstore.so ${DESTDIR} $(libdir)

