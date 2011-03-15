
import sys

sys.path.append("./")

import plugins

exchanges = nova_agent.exchange_plugins()
parsers = nova_agent.parser_plugins()

nova_agent.set_parser(exchanges, parsers[0])
