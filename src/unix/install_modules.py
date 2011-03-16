
import sys

import os
import shutil
import plugins

test_mode = True

if len(sys.argv) < 2:
    print "No destination directory specified"
    sys.exit(1)

dest_dir = sys.argv[1]

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
    os.mkdir(dest_dir)
except:
    pass

for i in to_install:
    if os.path.isdir(i):
        subdir = i.rsplit('/', 1)[1]
        shutil.copytree(i, "%s/%s" % (dest_dir, subdir))
    else:
        shutil.copy2(i, dest_dir)
