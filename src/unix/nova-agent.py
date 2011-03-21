
import sys
import agentlib
import plugins
import commands.command_list

test_mode = False

args = {"test": "test123"}
c = commands.init(**args)

parser = plugins.jsonparser(c)
xs = plugins.xenstore()

agentlib.register(xs, parser)
