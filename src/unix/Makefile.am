
include $(top_srcdir)/Common.am

ACLOCAL_MFLAGS = -I m4

SUBDIRS = src plugins

noinst_HEADERS = include/logging.h include/nova-agent.h \
				 include/plugin_int.h include/python.h
include_HEADERS = include/plugin.h

EXTRA_DIST = install_libs.py install_modules.py nova-agent.py

data_DATA = nova-agent.py

BINTARNAME = @PACKAGE@-`uname -m`-@PACKAGE_VERSION@.tar.gz

install-exec-local:
	@$(top_srcdir)/src/nova-agent -qS -- $(top_srcdir)/install_libs.py $(top_srcdir)/src/nova-agent ${DESTDIR}/$(libdir)
	@$(top_srcdir)/src/nova-agent -qS -- $(top_srcdir)/install_modules.py ${DESTDIR}/$(modulesdir)

distclean-local:
	rm -f $(top_srcdir)/{configure,configure.in}
	find $(top_srcdir) -name Makefile.in -exec rm {} \;
	find $(top_srcdir) -name configure.in -exec rm {} \;
	rm -f $(top_srcdir)/m4/{libtool.m4,ltoptions.m4,ltsugar.m4,ltversion.m4,lt~obsolete.m4}

clean-local::
	find . -name '*.gcov' -delete -o -name '*.gcda' -delete -o -name '*.gcno' -delete
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
