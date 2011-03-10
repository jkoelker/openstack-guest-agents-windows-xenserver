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
JSON misc commands plugin
"""

from plugins.jsonparser import jsonparser
import os
import platform

class network_commands(jsonparser.command):

    def __init__(self, *args, **kwargs):
        super(jsonparser.command, self).__init__(*args, **kwargs)

    @classmethod
    def detect_os(self):
        """
        Return the Linux Distribution or other OS name
        """

        translations = {"ubuntu": "debian",
                        "centos": "redhat",
                        "fedora": "redhat",
                        "oracle": "redhat"}

        system = os.uname()[0]
        if system == "Linux":
            system = platform.linux_distribution(None)[0]

        try:
            system = translations[system.lower()]
        except Exception:
            pass

        return system

    @jsonparser.command_add('resetnetwork')
    def resetnetwork_cmd(self, data):

        system = self.detect_os()
        if not system:
            raise SystemError("Couldn't figure out my OS")

        os_mod = __import__("%s" % system, globals(), locals(),
                ["network"], -1)

        return os_mod.network.configure_network(data)
