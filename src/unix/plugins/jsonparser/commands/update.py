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
JSON agent update handling plugin
"""

import os
import urllib
from plugins.jsonparser import jsonparser

TMP_PATH = "/tmp"

try:
    import hashlib
except ImportError:
    import md5

    class hashlib(object):
        @staticmethod
        def md5():
            return md5.new()

class AgentUpdateError(Exception):

    def __init__(self, arg):
        self.arg = arg

    def __str__(self):
        return self.arg

    __repr__ = __str__


class update_command(jsonparser.command):

    def __init__(self, *args, **kwargs):
        super(jsonparser.command, self).__init__(*args, **kwargs)

    def _get_to_local_file(self, url, md5sum):
        try:
            filename = url[url.rindex('/') + 1:]
            ext_pos = filename.index('.')
            local_filename = "%s/%s-%d%s" % (
                    TMP_PATH,
                    filename[:ext_pos],
                    os.getpid(),
                    filename[ext_pos:])
        except ValueError:
            raise AgentUpdateError("Invalid URL")

        try:
            urllib.urlretrieve(url, local_filename)
        except Exception, e:
            raise AgentUpdateError(str(e))

        try:
            f = file(local_filename, 'rb')
        except:
            os.unlink(local_filename)
            raise AgentUpdateError("Couldn't open local file")

        m = hashlib.md5()
        while True:
            file_data = f.read(8096)
            if not file_data:
                break
            m.update(file_data)

        digest = m.hexdigest()
        if digest != md5sum:
            os.unlink(local_filename)
            raise AgentUpdateError("MD5 sums do not match (%s != %s)" % (
                    md5sum, digest))

        return local_filename


    @jsonparser.command_add('agentupdate')
    def update_cmd(self, data):

        if isinstance(data, str):
            (url, md5sum) = data.split(',', 1)
        elif isinstance(data, dict):
            try:
                url = data['url']
                md5sum = data['md5sum']
            except KeyError:
                return (500,
                        "Missing URL or MD5 sum in dictionary arguments")
        else:
            return (500, "Invalid arguments")

        try:
            local_filename = self._get_to_local_file(url, md5sum)
        except AgentUpdateError, e:
            return (500, str(e))

        return (0, "")


