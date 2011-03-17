
import os
import shutil
import subprocess
import sys

# For nova_agent binary
test_mode = True

def install_libs_for_binary(binary, destdir):
    """
    Install all dynamic library dependencies for a binary
    """

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
        print "Installing %s into %s" % (lib, destdir)
        shutil.copy2(lib, destdir)

if len(sys.argv) != 3:
    print "Usage: install_libs.py <binary_name> <dest_dir>"
    sys.exit(1)

binary = sys.argv[1]
destdir = sys.argv[2]

if not os.path.exists(destdir):
    os.mkdir(destdir)
elif not os.path.isdir(destdir):
    print "Error: '%s' exists and is not a directory" % destdir
    sys.exit(1)

install_libs_for_binary(binary, destdir)
