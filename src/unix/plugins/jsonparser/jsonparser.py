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

class command_metaclass(type):
    def __init__(cls, cls_name, bases, attrs):
        if not hasattr(cls, '_command_classes'):
            cls._command_classes = []
            cls._command_instances = []
        else:
            cls._command_classes.append(cls)

class command(object):
    """
    The class that all command classes should inherit from
    """

    # Set the metaclass
    __metaclass__ = command_metaclass

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

        self.commands = {}
        self.command_instances = []

        for cls in command._command_classes:
            inst = cls(*args, **kwargs)
            for objname in dir(cls):
                obj = getattr(cls, objname)
                if getattr(obj, '_is_cmd', False):
                    self.commands[obj._cmd_name] = getattr(inst, objname)
            self.command_instances.append(cls)

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
            cmd_func = self.commands[cmd_name]
        except KeyError:
            print "No such command %s" % cmd_name
            return None

        try:
            cmd_string = request['value']
        except KeyError:
            cmd_string = ''

        result = cmd_func(cmd_string)

        return {"data": anyjson.serialize(result)}
