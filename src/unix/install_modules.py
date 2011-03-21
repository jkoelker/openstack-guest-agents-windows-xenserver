#!/usr/bin/env python

import os
import shutil
import subprocess
import sys


def install_plugins(destdir):

#    import plugins
    import commands.command_list

    c = commands.init({"testmode": True})

    to_install = set()

    def copy_tree(srcdir, destdir):
        print "Installing %s" % srcdir
        if not os.path.exists(destdir):
            os.mkdir(destdir)
        for root, dirs, files in os.walk(srcdir):
            for d in dirs:
                if not os.path.exists(os.path.join(destdir, d)):
                    os.mkdir(os.path.join(destdir, d))
            for f in files:
                # Only install .pyc or .sos, etc
                if not f.endswith('.py'):
                    shutil.copy2(os.path.join(root, f),
                            os.path.join(destdir + root[len(srcdir):], f))


    for modname in sys.modules:
        try:
            mod_fn = __import__(modname).__file__
        except:
            continue

        (mod_dir, mod_file) = mod_fn.rsplit('/', 1)

        if mod_dir == "%s/%s" % (sys.path[0], "plugins"):
            # Skip our plugins.
            continue

        if mod_dir in sys.path:
            to_install.add(mod_fn)
        else:
            to_install.add(mod_dir)

    try:
        os.mkdir(destdir)
    except:
        pass

    for i in to_install:
        if os.path.isdir(i):
            if i.endswith('.'):
                continue
            subdir = i.rsplit('/', 1)[1]
            copy_tree(i, os.path.join(destdir, subdir))
        else:
            print "Installing %s" % i
            shutil.copy2(i, destdir)


if len(sys.argv) != 2:
    print "Usage: install_modules.py <dest_dir>"
    sys.exit(1)

destdir = sys.argv[1]

if not os.path.exists(destdir):
    os.makedirs(destdir)
elif not os.path.isdir(destdir):
    print "Error: '%s' exists and is not a directory" % destdir
    sys.exit(1)

install_plugins(destdir)
