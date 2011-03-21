
include $(top_srcdir)/Common.am

ACLOCAL_MFLAGS = -I m4

AM_CFLAGS += -DAGENT_VERSION=\"@PACKAGE_VERSION@\" -DPLUGINS_DIR=\"${pluginsdir}\" -DDATA_DIR=\"${datadir}\" -fvisibility=hidden

SUBDIRS = lib plugins

EXTRA_DIST = install_libs.py install_modules.py nova-agent.py

#
# agentlib library
#

noinst_HEADERS = lib/agentlib_int.h lib/plugin_int.h
include_HEADERS = lib/agentlib.h lib/plugin.h

nobase_lib_LTLIBRARIES = lib/libagentlib.la

lib_libagentlib_la_SOURCES = lib/agentlib.c lib/plugin.c

#
# nova-agent binary
#

noinst_HEADERS += include/logging_int.h include/nova-agent_int.h \
                 include/python_int.h
include_HEADERS += include/logging.h include/nova-agent.h

sbin_PROGRAMS = src/nova-agent

src_nova_agent_SOURCES = src/nova-agent.c src/python.c src/logging.c

src_nova_agent_LDADD = @PYTHON_LIB@ lib/libagentlib.la -lpthread
src_nova_agent_LDFLAGS = -export-dynamic


nodist_noinst_DATA = lib/agentlib.so

lib/agentlib.so:
	    ln -s .libs/libagentlib.so lib/agentlib.so

data_DATA = nova-agent.py

BINTARNAME = @PACKAGE@-`uname -m`-@PACKAGE_VERSION@.tar.gz

install-exec-local:
	@$(top_srcdir)/src/nova-agent -qS -l error -- $(top_srcdir)/install_libs.py $(top_srcdir)/src/nova-agent ${DESTDIR} $(libdir)
	@$(top_srcdir)/src/nova-agent -qS -l error -- $(top_srcdir)/install_modules.py ${DESTDIR}/$(modulesdir)
	@./patch_binary.py ${DESTDIR}${sbindir}/nova-agent ${DESTDIR} $(libdir)

distclean-local:
	rm -f $(top_srcdir)/{configure,configure.in}
	rm -f $(top_srcdir)/{aclocal.m4,compile,config.guess,config.sub}
	rm -f $(top_srcdir)/{depcomp,install-sh,ltmain.sh,missing}
	find $(top_srcdir) -name Makefile.in -exec rm -f {} \;
	find $(top_srcdir) -name configure.in -exec rm -f {} \;
	rm -rf $(top_srcdir)/m4
	rm -rf $(top_srcdir)/autom4te.cache

clean-local::
	rm -f lib/agentlib.so
	find . -name '*.gcov' -delete -o -name '*.gcda' -delete -o -name '*.gcno' -delete
	find . -name '*.pyc' -exec rm -f {} \;
	rm -rf $(CURDIR)/fakeroot

coverage:: check
	for d in $(SUBDIRS) ; do cd $$d ; for f in *.c ; do gcov $$f; done ; cd ..; done

bintar: all
	@rm -rf fakeroot; mkdir -p fakeroot
	@$(MAKE) install DESTDIR=$(CURDIR)/fakeroot
	@find fakeroot -type f | sed -e 's@^fakeroot@@g' > filelist ; mv filelist $(datadir)
	@tar -czC fakeroot -f $(BINTARNAME) .
	@echo ""
	@echo "Created $(BINTARNAME)"
	@echo ""
