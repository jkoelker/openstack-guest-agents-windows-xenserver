
import os
import shutil
import subprocess
import sys

# For nova_agent binary
test_mode = True


def install_plugins(destdir):

    import plugins

    to_install = set()
    
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
            subdir = i.rsplit('/', 1)[1]
            shutil.copytree(i, "%s/%s" % (destdir, subdir))
        else:
            shutil.copy2(i, destdir)


if len(sys.argv) != 2:
    print "Usage: install_modules.py <dest_dir>"
    sys.exit(1)

destdir = sys.argv[1]

if not os.path.exists(destdir):
    os.mkdir(destdir)
elif not os.path.isdir(destdir):
    print "Error: '%s' exists and is not a directory" % destdir
    sys.exit(1)

install_plugins(destdir)
