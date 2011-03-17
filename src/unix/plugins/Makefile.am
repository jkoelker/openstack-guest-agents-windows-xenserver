
include $(top_srcdir)/Common.am

dist_plugins_SCRIPTS = __init__.py

nobase_dist_plugins_SCRIPTS = xenstore/__init__.py \
							  jsonparser/__init__.py \
							  jsonparser/jsonparser.py \
							  jsonparser/commands/__init__.py \
							  jsonparser/commands/file_inject.py \
							  jsonparser/commands/misc.py \
							  jsonparser/commands/network.py \
							  jsonparser/commands/password.py \
							  jsonparser/commands/update.py \
							  jsonparser/commands/debian/__init__.py \
							  jsonparser/commands/debian/network.py \
							  jsonparser/commands/redhat/__init__.py \
							  jsonparser/commands/redhat/network.py

nobase_pkglib_LTLIBRARIES=xenstore/xenstore.la

xenstore_xenstore_la_SOURCES=xenstore/xenstore.c
xenstore_xenstore_la_LIBADD = -lxenstore
xenstore_xenstore_la_LDFLAGS = -module -avoid-version

nodist_noinst_DATA = xenstore/xenstore.so

xenstore/xenstore.so:
	ln -s .libs/xenstore.so xenstore/xenstore.so

clean-local:
	rm -f xenstore/xenstore.so

