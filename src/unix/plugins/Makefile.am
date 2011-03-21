
include $(top_srcdir)/Common.am

dist_plugins_SCRIPTS = __init__.py jsonparser.py

nobase_pkglib_LTLIBRARIES = xenstore.la

xenstore_la_SOURCES = xenstore.c
xenstore_la_LIBADD = $(top_srcdir)/lib/libagentlib.la -lxenstore
xenstore_la_LDFLAGS = -module -avoid-version

nodist_noinst_DATA = xenstore.so

xenstore.so:
	ln -s .libs/xenstore.so xenstore.so

$(top_srcdir)/lib/libagentlib.la:
	cd $(top_srcdir)/lib && make

clean-local:
	rm -f xenstore.so

install-exec-local:
	@$(top_srcdir)/install_libs.py xenstore.so ${DESTDIR} $(libdir)

