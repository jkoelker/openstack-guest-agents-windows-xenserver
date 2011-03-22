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

import os
import platform

import commands
import debian.network
import redhat.network


class NetworkCommands(commands.CommandBase):

    def __init__(self, *args, **kwargs):
        pass

    @classmethod
    def detect_os(self):
        """
        Return the Linux Distribution or other OS name
        """

        translations = {"debian": debian,
                        "ubuntu": debian,
                        "redhat": redhat,
                        "centos": redhat,
                        "fedora": redhat,
                        "oracle": redhat}

        system = os.uname()[0]
        if system == "Linux":
            try:
                system = platform.linux_distribution(None)[0]
            except AttributeError:
                # linux_distribution doesn't exist... try the older
                # call
                system = platform.dist(None)[0]

        return translations.get(system.lower())

    @commands.command_add('resetnetwork')
    def resetnetwork_cmd(self, data):

        os_mod = self.detect_os()
        if not os_mod:
            raise SystemError("Couldn't figure out my OS")

        return os_mod.network.configure_network(data)
