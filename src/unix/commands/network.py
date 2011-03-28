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
import re
import time
import platform
from cStringIO import StringIO

import commands
import debian.network
import redhat.network
import arch.network
import suse.network
import gentoo.network

HOSTS_FILE = '/etc/hosts'


class NetworkCommands(commands.CommandBase):

    def __init__(self, *args, **kwargs):
        pass

    @staticmethod
    def detect_os():
        """
        Return the Linux Distribution or other OS name
        """

        translations = {"debian": debian,
                        "ubuntu": debian,
                        "redhat": redhat,
                        "centos": redhat,
                        "fedora": redhat,
                        "oracle": redhat,
                        "arch": arch,
                        "opensuse": suse,
                        "gentoo": gentoo}

        system = os.uname()[0]
        if system == "Linux":
            try:
                system = platform.linux_distribution(None)[0]
            except AttributeError:
                # linux_distribution doesn't exist... try the older
                # call
                system = platform.dist(None)[0]

            # Gentoo returns 'Gentoo Base System', so let's make that
            # something easier to use
            if system:
                system = system.lower().split(' ')[0]

            # Arch Linux returns None for platform.linux_distribution()
            if not system and os.path.exists('/etc/arch-release'):
                system = 'arch'

        if not system:
            return None

        return translations.get(system)

    @commands.command_add('resetnetwork')
    def resetnetwork_cmd(self, data):

        os_mod = self.detect_os()
        if not os_mod:
            raise SystemError("Couldn't figure out my OS")

        return os_mod.network.configure_network(data)


def _update_etc_hosts(infile, ips, hostname):
    outfile = StringIO()

    for line in infile:
        line = line.strip()

        if '#' in line:
            config, comment = line.split('#', 1)
            config = config.strip()
            comment = '\t#' + comment
        else:
            config, comment = line, ''

        parts = re.split('\s+', config)
        if parts:
            if parts[0] in ips:
                confip = parts.pop(0)
                if len(parts) == 1 and parts[0] != hostname:
                    # Single hostname that differs, we replace that one
                    print >> outfile, '# %s\t# Removed by nova-agent' % line
                    print >> outfile, '%s\t%s%s' % (confip, hostname, comment)
                elif len(parts) == 2 and len(
                        filter(lambda h: '.' in h, parts)) == 1:
                    # Two hostnames, one a hostname, one a domain name. Replace
                    # the hostname
                    hostnames = map(
                            lambda h: ('.' in h) and h or hostname, parts)
                    print >> outfile, '# %s\t# Removed by nova-agent' % line
                    print >> outfile, '%s\t%s%s' % (confip,
                            ' '.join(hostnames), comment)
                else:
                    # Don't know how to handle this line, so skip it
                    print >> outfile, line

                ips.remove(confip)
            else:
                print >> outfile, line
        else:
            print >> outfile, line

    # Add public IPs we didn't manage to patch
    for ip in ips:
        print >> outfile, '%s\t%s' % (ip, hostname)

    return outfile


def update_etc_hosts(ips, hostname, dont_rename=False):
    filename = HOSTS_FILE
    tmp_file = filename + ".%d~" % os.getpid()
    bak_file = filename + ".%d.bak" % time.time()

    outfile = _update_etc_hosts(open(filename), ips, hostname)
    outfile.seek(0)

    f = open(tmp_file, 'w')
    try:
        f.write(outfile.read())
        f.close()

        os.chown(tmp_file, 0, 0)
        os.chmod(tmp_file, 0644)
        if not dont_rename and os.path.exists(filename):
            os.rename(filename, bak_file)
    except Exception, e:
        os.unlink(tmp_file)
        raise e

    if not dont_rename:
        try:
            os.rename(tmp_file, filename)
            pass
        except Exception, e:
            os.rename(bak_file, filename)
            raise e
    else:
        os.rename(bak_file, filename)
