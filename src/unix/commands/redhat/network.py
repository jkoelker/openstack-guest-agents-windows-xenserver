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
from cStringIO import StringIO

import commands.network

HOSTNAME_FILE = "/etc/sysconfig/network"
NETCONFIG_DIR = "/etc/sysconfig/network-scripts"
INTERFACE_FILE = "ifcfg-%s"
ROUTE_FILE = "route-%s"

INTERFACE_LABELS = {"public": "eth0",
                    "private": "eth1"}


def configure_network(network_config, *args, **kwargs):

    interfaces = network_config.get('interfaces', [])

    publicips = write_interfaces(interfaces, dont_rename=0)

    hostname = network_config.get('hostname')

    update_hostname(hostname, dont_rename=False)
    commands.network.update_etc_hosts(publicips, hostname)

    # Set hostname
    logging.debug('executing /bin/hostname %s' % hostname)
    p = subprocess.Popen(["/bin/hostname", hostname])
    logging.debug('waiting on pid %d' % p.pid)
    status = os.waitpid(p.pid, 0)[1]
    logging.debug('status = %d' % status)

    if status != 0:
        return (500, "Couldn't set hostname: %d" % status)

    # Restart network
    logging.debug('executing /etc/init.d/network restart')
    p = subprocess.Popen(["/etc/init.d/network", "restart"])
    logging.debug('waiting on pid %d' % p.pid)
    status = os.waitpid(p.pid, 0)[1]
    logging.debug('status = %d' % status)

    if status != 0:
        return (500, "Couldn't restart network: %d" % status)

    return (0, "")


def _update_hostname(infile, hostname):
    """
    Update hostname on system
    """
    outfile = StringIO()

    found = False
    for line in infile:
        line = line.strip()
        if '=' in line:
            k, v = line.split('=', 1)
            k = k.strip()
            if k == "HOSTNAME":
                print >> outfile, "HOSTNAME=%s" % hostname
                found = True
            else:
                print >> outfile, line
        else:
            print >> outfile, line

    if not found:
        print >> outfile, "HOSTNAME=%s" % hostname

    return outfile


def update_hostname(hostname, dont_rename=False):
    """
    Update hostname on system
    """
    outfile = _update_hostname(open(HOSTNAME_FILE), hostname)
    outfile.seek(0)

    _write_file(HOSTNAME_FILE, outfile.read(), dont_rename)


def _update_interfaces(interfaces):
    results = {}

    for interface in interfaces:
        ifname, ifaces, route_data = _get_file_data(interface)

        for ifname, data in ifaces:
            results[INTERFACE_FILE % ifname] = data

        if route_data:
            results[ROUTE_FILE % ifname] = route_data

    return results


def write_interfaces(interfaces, *args, **kwargs):
    """
    Write out a new files for interfaces
    """

    dont_rename = kwargs.get('dont_rename', 0)

    publicips = set()

    # Enumerate all of the existing ifcfg-* files
    old_files = set()
    for filepath in glob.glob(NETCONFIG_DIR + "/ifcfg-*"):
        if '.' not in filepath:
            old_files.add(filepath)
    for filename in glob.glob(NETCONFIG_DIR + "/route-*"):
        if '.' not in filepath:
            old_files.add(filepath)

    lo_file = os.path.join(NETCONFIG_DIR, INTERFACE_FILE % 'lo')
    if lo_file in old_files:
        old_files.remove(lo_file)

    for filename, data in _update_interfaces(interfaces).iteritems():
        filepath = os.path.join(NETCONFIG_DIR, filename)

        logging.info("writing %s" % filepath)
        _write_file(filepath, data, dont_rename=dont_rename)

        if filepath in old_files:
            old_files.remove(filepath)

    for filepath in old_files:
        logging.info("moving aside old file %s" % filepath)
        if not dont_rename:
            os.rename(filepath, filepath + ".%d.bak" % time.time())

    for interface in interfaces:
        if not publicips and interface['label'] == 'public':
            ips = interface.get('ips')
            if ips:
                publicips.add(ips[0]['ip'])

            ip6s = interface.get('ip6s')
            if ip6s:
                publicips.add(ip6s[0]['address'])

    return publicips


def _write_file(filename, data, dont_rename=0):
    # Make sure we don't pick filenames that the init script will confuse
    # as real configuration files
    tmp_file = filename + ".%d~" % os.getpid()
    bak_file = filename + ".%d.bak" % time.time()

    f = open(tmp_file, 'w')
    try:
        f.write(data)
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

    ip4s = interface.get('ips', [])
    ip6s = interface.get('ip6s', [])

    if not ip4s and not ip6s:
        raise SystemError("No IPs found for interface")

    try:
        mac = interface['mac']
    except KeyError:
        raise SystemError("No mac address found for interface")

    if label == "public":
        gateway4 = interface.get('gateway')
        gateway6 = interface.get('gateway6')

        if not gateway4 and not gateway6:
            raise SystemError("No gateway found for public interface")

        try:
            dns = interface['dns']
        except KeyError:
            raise SystemError("No DNS found for public interface")

    ifaces = []

    ifname_suffix_num = 0

    for i in xrange(max(len(ip4s), len(ip6s))):
        if ifname_suffix_num:
            ifname = "%s:%d" % (ifname_prefix, ifname_suffix_num)
        else:
            ifname = ifname_prefix

        iface_data = "# Automatically generated, do not edit\n"
        iface_data += "DEVICE=%s\n" % ifname
        iface_data += "BOOTPROTO=static\n"
        iface_data += "HWADDR=%s\n" % mac

        if i < len(ip4s):
            ip_info = ip4s[i]

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
                if label == "public" and gateway4:
                    iface_data += "DEFROUTE=yes\n"
                    iface_data += "GATEWAY=%s\n" % gateway4

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

                gateway = ip_info.get('gateway', gateway6)

                iface_data += "IPV6INIT=yes\n"
                iface_data += "IPV6_AUTOCONF=no\n"
                iface_data += "IPV6ADDR=%s/%s\n" % (ip, netmask)

                if gateway:
                    iface_data += "IPV6_DEFAULTGW=%s\n" % gateway

        if gateway4 or gateway6:
            for j, nameserver in enumerate(dns):
                iface_data += "DNS%d=%s\n" % (j + 1, nameserver)

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
