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
JSON password reset handling plugin
"""

import base64
import binascii
import hashlib
import math
import os
import subprocess
import time
from Crypto.Cipher import AES
from plugins.jsonparser import jsonparser

class password_commands(jsonparser.command):

    def __init__(self, *args, **kwargs):
        super(jsonparser.command, self).__init__(*args, **kwargs)

    def _mod_exp(self, num, exp, mod):
        result = 1
        while exp > 0:
            if (exp & 1) == 1:
                result = (result * num) % mod
            exp = exp >> 1
            num = (num * num) % mod
        return result


    @jsonparser.command_add('keyinit')
    def keyinit_cmd(self, data):

        # Remote pubkey comes in as large number
        remote_public_key = data
        # prime to use
        prime = 162259276829213363391578010288127

        my_private_key = int(binascii.hexlify(os.urandom(16)), 16)
        my_public_key = self._mod_exp(5, my_private_key, prime)

        shared_key = str(self._mod_exp(remote_public_key,
                my_private_key, prime))

        m = hashlib.md5()
        m.update(shared_key)

        self.aes_key = m.digest()

        m = hashlib.md5()
        m.update(self.aes_key)
        m.update(shared_key)

        self.aes_iv = m.digest()

        # Needs to be a string response right now
        return (0, str(my_public_key))

    def change_password(self, passwd):

        try:
            p = subprocess.Popen(["/usr/sbin/chpasswd"],
                    stdin=subprocess.PIPE,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.PIPE)
            p.communicate("root:%s\n" % passwd)
            ret = p.wait()
            if ret:
                raise SystemError("Return code from chpasswd was %d" % ret)

        except Exception, e:
            p = subprocess.Popen(["/usr/bin/passwd", "root"],
                    stdin=subprocess.PIPE,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.PIPE)
            p.stdin.write("%s\n" % passwd)
            time.sleep(1)
            p.stdin.write("%s\n" % passwd)
            ret = p.wait()
            if ret:
                raise SystemError("Return code from passwd was %d" % ret)

    @jsonparser.command_add('password')
    def password_cmd(self, data):

        try:
            real_data = base64.b64decode(data)

            a = AES.new(self.aes_key, AES.MODE_CBC, self.aes_iv)
            passwd = a.decrypt(real_data)

        except Exception, e:
            print "Ignoring password without keyinit"
            return (500, "No keyinit")

        cut_off_sz = ord(passwd[len(passwd)-1])
        if cut_off_sz > 16:
            return (500, "Invalid password data received")

        passwd = passwd[:-cut_off_sz]

        try:
            self.change_password(passwd)
            return (0, "")
        except:
            return(500, "Couldn't change password")
