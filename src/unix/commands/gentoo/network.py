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
gentoo network helper module
"""

import os
import re
import time
import subprocess
import logging
from cStringIO import StringIO

import commands.network

HOSTNAME_FILE = "/etc/conf.d/hostname"
NETWORK_FILE = "/etc/conf.d/net"
RESOLV_FILE = "/etc/resolv.conf"

INTERFACE_LABELS = {"public": "eth0",
                    "private": "eth1"}


def configure_network(network_config, *args, **kwargs):

    interfaces = network_config.get('interfaces', [])

    network_data, ifaces, publicips = _get_file_data(interfaces)
    _write_file(NETWORK_FILE, network_data)

    resolv_data = _get_resolv_conf(interfaces)
    _write_file(RESOLV_FILE, resolv_data)

    hostname = network_config.get('hostname')

    hostname_data = _update_hostname(hostname)
    _write_file(HOSTNAME_FILE, hostname_data)

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
    for ifname in ifaces:
        logging.debug('executing /etc/init.d/net.%s restart' % ifname)
        p = subprocess.Popen(["/etc/init.d/net.%s" % ifname, "restart"])
        logging.debug('waiting on pid %d' % p.pid)
        status = os.waitpid(p.pid, 0)[1]
        logging.debug('status = %d' % status)

        if status != 0:
            return (500, "Couldn't restart network %s: %d" % (ifname, status))

    return (0, "")


def _update_hostname(hostname):
    """
    Update hostname on system
    """
    return '# Automatically generated, do not edit\nHOSTNAME="%s"\n' % hostname


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


def _parse_variable(line):
    k, v = line.split('=')
    v = v.strip()
    if v[0] == '(' and v[-1] == ')':
        v = v[1:-1]

    return [name.lstrip('!') for name in re.split('\s+', v.strip())]


def _get_resolv_conf(interfaces):
    resolv_data = ''
    for interface in interfaces:
        if interface['label'] != 'public':
            continue

        for nameserver in interface.get('dns', []):
            resolv_data += 'nameserver %s\n' % nameserver

    if not resolv_data:
        return ''

    return '# Automatically generated, do not edit\n' + resolv_data


def _update_interfaces(interfaces):
    data, ifaces, publicips = _get_file_data(interfaces)

    return {'net': data}


def _get_file_data(interfaces):
    """
    Return data for (sub-)interfaces and routes
    """

    ifaces = set()
    publicips = set()

    network_data = '# Automatically generated, do not edit\n'
    network_data += 'modules=( "ifconfig" )\n\n'

    for interface in interfaces:
        try:
            label = interface['label']
        except KeyError:
            raise SystemError("No interface label found")

        try:
            ifname = INTERFACE_LABELS[label]
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
        else:
            gateway4 = gateway6 = None

        network_data += 'config_%s=(\n' % ifname

        for ip_info in ip4s:
            enabled = ip_info.get('enabled', '0')
            if enabled != '1':
                continue

            try:
                ip = ip_info['ip']
                netmask = ip_info['netmask']
            except KeyError:
                raise SystemError(
                        "Missing IP or netmask in interface's IP list")

            network_data += '    "%s netmask %s"\n' % (ip, netmask)
    
        for ip_info in ip6s:
            enabled = ip_info.get('enabled', '0')
            if enabled != '1':
                continue

            try:
                ip = ip_info['address']
                netmask = ip_info['netmask']
            except KeyError:
                raise SystemError(
                        "Missing IP or netmask in interface's IP list")

            if not gateway6:
                gateway6 = ip_info.get('gateway', gateway6)

            network_data += '    "%s/%s"\n' % (ip, netmask)

        network_data += ')\n'

        routes = []
        for route in interface.get('routes', []):
            network = route['route']
            netmask = route['netmask']
            gateway = route['gateway']

            routes.append('%s netmask via %s' % (ip, netmask, gateway))

        if gateway4:
            routes.append('default via %s' % gateway4)
        if gateway6:
            routes.append('default via %s' % gateway6)

        if routes:
            network_data += 'routes_%s=(\n' % ifname
            for config in routes:
                network_data += '    "%s"\n' % config
            network_data += ')\n'

        ifaces.add(ifname)

        if not publicips and interface['label'] == 'public':
            ips = interface.get('ips')
            if ips:
                publicips.add(ips[0]['ip'])

            ip6s = interface.get('ip6s')
            if ip6s:
                publicips.add(ip6s[0]['address'])

    return (network_data, ifaces, publicips)
