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
debian/ubuntu network helper module
"""

import os
import time

INTERFACE_FILE = "/etc/network/interfaces"
TMP_INTERFACE_FILE = "%s.tmp.%d" % (INTERFACE_FILE, os.getpid())

INTERFACE_HEADER = \
"""
# Used by ifup(8) and ifdown(8). See the interfaces(5) manpage or
# /usr/share/doc/ifupdown/examples for more information.
# The loopback network interface
auto lo
iface lo inet loopback
"""

INTERFACE_LABELS = {"public": "eth0",
                    "private": "eth1"}


def configure_network(network_config, *args, **kwargs):

    try:
        hostname = network_config['hostname']
    except KeyError:
        hostname = None

    try:
        interfaces = network_config['interfaces']
    except KeyError:
        interfaces = []

    write_interfaces(interfaces, dont_rename=0)

    return (0, "")


def write_interfaces(interfaces, *args, **kwargs):
    """
    Write out a new interfaces file
    """

    try:
        dont_rename = kwargs['dont_rename']
    except KeyError:
        dont_rename = 0

    bak_file = INTERFACE_FILE + '.' + str(int(time.time()))
    tmp_file = TMP_INTERFACE_FILE

    data = _get_file_data(interfaces)

    f = open(tmp_file, 'w')
    f.write(data)
    f.close()

    try:
        os.chown(tmp_file, 0, 0)
        os.chmod(tmp_file, 0644)
        if not dont_rename:
            os.rename(INTERFACE_FILE, bak_file)
    except Exception, e:
        os.unlink(tmp_file)
        raise e

    if not dont_rename:
        try:
            os.rename(tmp_file, INTERFACE_FILE)
            pass
        except Exception, e:
            os.rename(bak_file, INTERFACE_FILE)
            raise e


def _get_file_data(interfaces):
    """
    Return interfaces file data in 1 long string
    """

    file_data = INTERFACE_HEADER

    for interface in interfaces:
        try:
            label = interface['label']
        except KeyError:
            raise SystemError("No interface label found")

        try:
            ifname_prefix = INTERFACE_LABELS[label]
        except KeyError:
            raise SystemError("Invalid label '%s'" % label)

        try:
            ips = interface['ips']
        except KeyError:
            raise SystemError("No IPs found for interface")

        try:
            mac = interface['mac']
        except KeyError:
            raise SystemError("No mac address found for interface")

        try:
            routes = interface['routes']
        except KeyError:
            routes = []

        if label == "public":
            try:
                gateway = interface['gateway']
            except KeyError:
                raise SystemError("No gateway found for public interface")

            try:
                dns = interface['dns']
            except KeyError:
                raise SystemError("No DNS found for public interface")

        ifname_suffix_num = 0

        for ip_info in ips:
            try:
                enabled = ip_info['enabled']
            except KeyError:
                enabled = "0"

            if enabled == "0":
                continue

            try:
                ip = ip_info['ip']
                netmask = ip_info['netmask']
            except KeyError:
                raise SystemError(
                        "Missing IP or netmask in interface's IP list")

            if ifname_suffix_num:
                ifname = "%s:%d" % (ifname_prefix, ifname_suffix_num)
            else:
                ifname = ifname_prefix

            file_data += "auto %s\n" % ifname
            file_data += "    address %s\n" % ip
            file_data += "    netmask %s\n" % netmask
            if label == "public":
                file_data += "    gateway %s\n" % gateway
                nameservers = ' '.join(dns)
                file_data += "    dns-nameservers %s\n" % nameservers
            file_data += "\n"

            ifname_suffix_num += 1

        for route in routes:
            network = route['route']
            netmask = route['netmask']
            gateway = route['gateway']

            file_data += "up route add -net %s netmask %s gw %s\n" % (
                    network, netmask, gateway)
            file_data += "down route del -net %s netmask %s gw %s\n" % (
                    network, netmask, gateway)

    return file_data
