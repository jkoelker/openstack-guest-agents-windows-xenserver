
import sys
import plugins

test_mode = False

exchanges = nova_agent.exchange_plugins()
parsers = nova_agent.parser_plugins()

nova_agent.set_parser(exchanges, parsers[0])
