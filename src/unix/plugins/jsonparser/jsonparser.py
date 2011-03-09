# vim: tabstop=4 shiftwidth=4 softtabstop=4
# 
#  Copyright (c) 2011 Openstack, LLC.
#  All Rights Reserved.
# 
#     Licensed under the Apache License, Version 2.0 (the "License"); you may
#     not use this file except in compliance with the License. You may obtain
#     a copy of the License at
# 
#          http://www.apache.org/licenses/LICENSE-2.0
# 
#     Unless required by applicable law or agreed to in writing, software
#     distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
#     WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
#     License for the specific language governing permissions and limitations
#     under the License.
#

"""
JSON agent command parser main code module
"""

import nova_agent
import anyjson

class CommandNotFoundError(Exception):
    def __init__(self, cmd):
        self.cmd = cmd

    def __str__(self):
        return "No such agent command '%s'" % self.cmd


class command_metaclass(type):
    def __init__(cls, cls_name, bases, attrs):
        if not hasattr(cls, '_cmd_classes'):
            cls._cmd_classes = []
            cls._cmd_instances = []
        else:
            cls._cmd_classes.append(cls)

class command(object):
    """
    The class that all command classes should inherit from
    """

    # Set the metaclass
    __metaclass__ = command_metaclass

    @classmethod
    def create_instances(self, *args, **kwargs):
        for cls in self._cmd_classes:
            self._cmd_instances.append(cls(*args, **kwargs))

    @classmethod
    def get_commands(self):
        cmds = {}
        for inst in self._cmd_instances:
            for objname in dir(inst):
                obj = getattr(inst, objname)
                if getattr(obj, '_is_cmd', False):
                    try:
                        cmds[obj._cmd_name] = obj
                    except AttributeError:
                        # skip it if there's no _cmd_name
                        pass
        return cmds

def command_add(cmd_name):
    """
    Decorator for command classes to use to add commands
    """

    def wrap(f):
        f._is_cmd = True
        f._cmd_name = cmd_name
        return f
    return wrap

class command_parser(nova_agent.plugin):
    """
    JSON command parser plugin for nova-agent
    """

    type = "parser"

    def __init__(self, *args, **kwargs):
        super(command_parser, self).__init__(*args, **kwargs)

        __import__("plugins.jsonparser.commands")
        command.create_instances()
        self._commands = command.get_commands()

    def run_command(self, cmd_name, arg):

        try:
            result = self._commands[cmd_name](arg)
        except KeyError:
            raise CommandNotFoundError(cmd_name)

        return result

    def encode_result(self, result):

        our_format = {"returncode": str(result[0]),
                      "message": result[1]}

        return {"data": anyjson.serialize(our_format)}

    def parse_request(self, request):

        try:
            request = anyjson.deserialize(request['data'])
        except Exception, e:
            # log it
            print "Missing data"
            print e
            return None

        try:
            cmd_name = request['name']
        except KeyError:
            print "Missing command name"
            return None

        try:
            cmd_string = request['value']
        except KeyError:
            cmd_string = ''

        try:
            result = self.run_command(cmd_name, cmd_string)
        except CommandNotFoundError, e:
            print e
            return self.encode_result((404, str(e)))

        return self.encode_result(result)
