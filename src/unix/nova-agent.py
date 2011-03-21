
import sys
import lib.agentlib
import plugins.jsonparser
import plugins.xenstore
import commands.command_list

test_mode = False

args = {"test": "test123"}
c = commands.init(args)

sys.exit(0)

parser = plugins.jsonparser.jsonparser(c)
xs = plugins.xenstore.xenstore()

lib.agentlib.register(xs, parser)


