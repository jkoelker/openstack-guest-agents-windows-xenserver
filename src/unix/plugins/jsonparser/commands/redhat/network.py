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
redhat/centos network helper module
"""

import os
import time
import glob
import subprocess
import logging

NETCONFIG_DIR = "/etc/sysconfig/network-scripts"
INTERFACE_FILE = NETCONFIG_DIR + "/ifcfg-%s"
ROUTE_FILE = NETCONFIG_DIR + "/route-%s"

INTERFACE_LABELS = {"public": "eth0",
                    "private": "eth1"}


def configure_network(network_config, *args, **kwargs):

    hostname = network_config.get('hostname')

    interfaces = network_config.get('interfaces', [])

    write_interfaces(interfaces, dont_rename=0)

    logging.debug('executing /etc/init.d/network restart')
    p = subprocess.Popen(["/etc/init.d/network", "restart"])
    logging.debug('waiting on pid %d' % p.pid)
    status = os.waitpid(p.pid, 0)[1]
    logging.debug('status = %d' % status)

    if status != 0:
        return (500, "Couldn't restart network: %d" % status)

    return (0, "")


def write_interfaces(interfaces, *args, **kwargs):
    """
    Write out a new files for interfaces
    """

    dont_rename = kwargs.get('dont_rename', 0)

    # Enumerate all of the existing ifcfg-* files
    old_files = set()
    for filename in glob.glob(NETCONFIG_DIR + "/ifcfg-*"):
        if '.' not in filename:
            old_files.add(filename)
    for filename in glob.glob(NETCONFIG_DIR + "/route-*"):
        if '.' not in filename:
            old_files.add(filename)

    lo_file = INTERFACE_FILE % 'lo'
    if lo_file in old_files:
        old_files.remove(lo_file)

    for interface in interfaces:
        ifname, ifaces, route_data = _get_file_data(interface)

        route_file = ROUTE_FILE % ifname
        if route_data:
            logging.info("writing %s" % route_file)
            _write_file(route_file, route_data, dont_rename=dont_rename)

            if route_file in old_files:
                old_files.remove(route_file)

        for ifname, data in ifaces:
            iface_file = INTERFACE_FILE % ifname
            logging.info("writing %s" % iface_file)
            _write_file(iface_file, data, dont_rename=dont_rename)

            if iface_file in old_files:
                old_files.remove(iface_file)

    for filename in old_files:
        logging.info("moving aside old file %s" % filename)
        if not dont_rename:
            os.rename(filename, filename + ".%d" % time.time() + ".bak")


def _write_file(filename, data, dont_rename=0):
    # Make sure we don't pick filenames that the init script will confuse
    # as real configuration files
    tmp_file = filename + ".%d~" % os.getpid()
    bak_file = filename + ".%d" % time.time() + ".bak"

    f = open(tmp_file, 'w')
    f.write(data)
    f.close()

    try:
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


def _get_file_data(interface):
    """
    Return data for (sub-)interfaces and routes
    """

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

    ip6s = interface.get('ip6s', [])

    try:
        mac = interface['mac']
    except KeyError:
        raise SystemError("No mac address found for interface")

    if label == "public":
        try:
            gateway = interface['gateway']
        except KeyError:
            raise SystemError("No gateway found for public interface")

        try:
            dns = interface['dns']
        except KeyError:
            raise SystemError("No DNS found for public interface")

    ifaces = []

    ifname_suffix_num = 0

    for i in xrange(max(len(ips), len(ip6s))):
        if ifname_suffix_num:
            ifname = "%s:%d" % (ifname_prefix, ifname_suffix_num)
        else:
            ifname = ifname_prefix

        iface_data = "# Automatically generated, do not edit\n"
        iface_data += "DEVICE=%s\n" % ifname
        iface_data += "BOOTPROTO=static\n"
        iface_data += "HWADDR=%s\n" % mac

        if i < len(ips):
            ip_info = ips[i]

            enabled = ip_info.get('enabled', '0')
            if enabled != '0':
                try:
                    ip = ip_info['ip']
                    netmask = ip_info['netmask']
                except KeyError:
                    raise SystemError(
                            "Missing IP or netmask in interface's IP list")

                iface_data += "IPADDR=%s\n" % ip
                iface_data += "NETMASK=%s\n" % netmask
                if label == "public":
                    iface_data += "DEFROUTE=yes\n"
                    iface_data += "GATEWAY=%s\n" % gateway
                    for j, nameserver in enumerate(dns):
                        iface_data += "DNS%d=%s\n" % (j + 1, nameserver)

        if i < len(ip6s):
            ip_info = ip6s[i]

            enabled = ip_info.get('enabled', '0')
            if enabled != '0':
                try:
                    ip = ip_info['address']
                    netmask = ip_info['netmask']
                except KeyError:
                    raise SystemError(
                            "Missing IP or netmask in interface's IP list")

                gateway = ip_info.get('gateway')

                iface_data += "IPV6INIT=yes\n"
                iface_data += "IPV6_AUTOCONF=no\n"
                iface_data += "IPV6ADDR=%s/%s\n" % (ip, netmask)

                if gateway:
                    iface_data += "IPV6_DEFAULTGW=%s\n" % gateway

        iface_data += "ONBOOT=yes\n"
        iface_data += "NM_CONTROLLED=no\n"
        ifname_suffix_num += 1

        ifaces.append((ifname, iface_data))

    route_data = ''
    for route in interface.get('routes', []):
        network = route['route']
        netmask = route['netmask']
        gateway = route['gateway']

        route_data += "-net %s netmask %s gw %s\n" % (
                network, netmask, gateway)

    return (ifname_prefix, ifaces, route_data)
