
import sys
import lib.agentlib
import plugins.jsonparser
import plugins.xenstore
import commands.command_list

test_mode = False

args = {"test": "test123"}
c = commands.init(args)

parser = plugins.jsonparser.jsonparser.command_parser(c)
xs = plugins.xenstore.xenstore.xenstore()

lib.agentlib.register(xs, parser)


