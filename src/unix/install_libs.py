#!/usr/bin/env python

import os
import shutil
import subprocess
import sys


def install_libs_for_binary(binary, destdir, libdir):
    """
    Install all dynamic library dependencies for a binary
    """

    installdir = destdir + libdir

    def _find_libs(target):
        """
        Use ldd on a binary/library to find out its dynamic libraries.
        """

        p = subprocess.Popen(["ldd", target],
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE)
        (outdata, errdata) = p.communicate()

        libs = set()

        for line in outdata.split('\n'):
            fields = line.split()

            if not len(fields):
                continue

            if len(fields) > 2 and os.path.exists(fields[2]):
                    libs.add(fields[2])
            elif os.path.exists(fields[0]):
                    libs.add(fields[0])

        return libs

    def find_libs(target):
        """
        Get a list of libraries for a target.  Recurse through
        those libraries to find other libraries.
        """

        libs = set()
        more_libs = _find_libs(target)

        while libs != more_libs:
            for lib in set(more_libs - libs):
                libs.add(lib)
                more_libs.update(_find_libs(lib))

        return libs

    for lib in find_libs(binary):
        if lib.startswith(installdir):
            # Already installed
            continue
        print "Installing %s" % lib
        shutil.copy2(lib, installdir)

        filename = os.path.basename(lib)
        if not filename.startswith('ld-'):
            args = ['patchelf', '--set-rpath', libdir,
                    os.path.join(installdir, filename)]
            p = subprocess.Popen(args)
            status = os.waitpid(p.pid, 0)[1]

            if status:
                raise Exception(
                        "failed to execute %s: status %d" % ' '.join(args),
                        status)

if len(sys.argv) != 4:
    print "Usage: install_libs.py <binary_name> <dest_dir> <lib_dir>"
    sys.exit(1)

binary = sys.argv[1]
destdir = os.path.normpath(sys.argv[2])
libdir = os.path.normpath(sys.argv[3])

installdir = destdir + libdir

if not os.path.exists(installdir):
    os.makedirs(installdir)
elif not os.path.isdir(installdir):
    print "Error: '%s' exists and is not a directory" % installdir
    sys.exit(1)

install_libs_for_binary(binary, destdir, libdir)
