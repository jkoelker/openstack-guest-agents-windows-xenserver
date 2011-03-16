
include $(top_srcdir)/Common.am

ACLOCAL_MFLAGS = -I m4

SUBDIRS = src plugins
BINTARNAME = @PACKAGE@-`uname -m`-@PACKAGE_VERSION@.tar.gz
EXTRA_DIST = 

install-exec-local:
	src/nova-agent -- install_modules.py $(modulesdir)
	sh install_libs.sh $(libdir)

clean-local::
	find . -name '*.gcov' -delete -o -name '*.gcda' -delete -o -name '*.gcno' -delete
	rm -rf $(CURDIR)/fakeroot

coverage:: check
	for d in $(SUBDIRS) ; do cd $$d ; for f in *.c ; do gcov $$f; done ; cd ..; done

bintar: all
	@rm -rf fakeroot; mkdir -p fakeroot
	@$(MAKE) install DESTDIR=$(CURDIR)/fakeroot
	@find fakeroot -type f | sed -e 's@^fakeroot@@g' > filelist ; mv filelist fakeroot/usr/share/agent-smith
	@tar -czC fakeroot -f $(BINTARNAME) .
	@echo ""
	@echo "Created $(BINTARNAME)"
	@echo ""
