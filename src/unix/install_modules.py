#!/usr/bin/env python

import os
import shutil
import subprocess
import sys

import commands.command_list


def install_plugins(destdir):

    c = commands.init(testmode=True)

    to_install = set()

    def copy_tree(srcdir, destdir):
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

        if modname == "__main__":
            continue

        try:
            mod_fn = sys.modules[modname].__file__
        except:
            continue

        mod_fn = os.path.normpath(mod_fn)

        base_dir = ''

        for p in sys.path:
            p_len = len(p)

            if mod_fn.startswith(p) and p > len(base_dir):
                base_dir = p

        # Skip modules that are not in sys.patH, as they must be
        # our modules and we install those separately.

        if base_dir:
            # Turn /usr/lib/python2.6/Crypto/Cipher/AES into:
            # /usr/lib/python2.6/Crypto
            rest_dir = mod_fn[len(base_dir) + 1:]
            if '/' in rest_dir:
                rest_dir = rest_dir.split('/', 1)[0]
            to_install.add(os.path.join(base_dir, rest_dir))

    try:
        os.mkdir(destdir)
    except:
        pass

    for i in to_install:
        print "Installing %s" % i
        if os.path.isdir(i):
            subdir = i.rsplit('/', 1)[1]
            copy_tree(i, os.path.join(destdir, subdir))
        else:
            shutil.copy2(i, destdir)


if len(sys.argv) != 2:
    print "Usage: install_modules.py <dest_dir>"
    sys.exit(1)

# Pop the first directory off that python uses
# It adds this directory (the one this script is in)
# We do this in order to skip our modules
sys.path.pop(0)

destdir = os.path.normpath(sys.argv[1])

if not os.path.exists(destdir):
    os.makedirs(destdir)
elif not os.path.isdir(destdir):
    print "Error: '%s' exists and is not a directory" % destdir
    sys.exit(1)

install_plugins(destdir)
