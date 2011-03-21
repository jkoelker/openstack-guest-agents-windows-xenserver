
import sys
import plugins.jsonparser
import plugins.xenstore
import commands
import lib.agentlib

test_mode = False

args = {}
c = commands.init(args)

parser = plugins.jsonparser.jsonparser(c)
xs = plugins.xenstore.xenstore()

lib.agentlib.register(xs, parser)


