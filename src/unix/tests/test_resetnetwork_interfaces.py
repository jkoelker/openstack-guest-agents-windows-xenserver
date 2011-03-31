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
resetnetwork interfaces tester
"""

import os
import unittest
from cStringIO import StringIO

import commands.redhat.network
import commands.debian.network


class TestInterfacesUpdates(unittest.TestCase):

    def _run_test(self, dist, **config):
        interfaces = []
        for label, options in config.iteritems():
            interface = {'label': label, 'mac': options['hwaddr']}

            ip4s = []
            for ip, netmask in options.get('ipv4', []):
                ip4s.append({'enabled': '1', 'ip': ip, 'netmask': netmask})
            interface['ips'] = ip4s

            if options.get('gateway4'):
                interface['gateway'] = options['gateway4']

            ip6s = []
            for ip, netmask in options.get('ipv6', []):
                ip6s.append({'enabled': '1', 'address': ip,
                    'netmask': netmask})
            interface['ip6s'] = ip6s

            if options.get('gateway6'):
                interface['gateway6'] = options['gateway6']

            if options.get('dns'):
                interface['dns'] = options['dns']

            interfaces.append(interface)

        return getattr(commands, dist).network._update_interfaces(interfaces)

    def test_redhat_ipv4(self):
        """Test setting public IPv4 for Red Hat networking"""
        interface = {
            'hwaddr': '00:11:22:33:44:55',
            'ipv4': [('192.0.2.42', '255.255.255.0')],
            'gateway4': '192.0.2.1',
            'dns': ['192.0.2.2'],
        }
        outfiles = self._run_test('redhat', public=interface)
        self.assertTrue('ifcfg-eth0' in outfiles)
        self.assertEqual(outfiles['ifcfg-eth0'], '\n'.join([
            '# Automatically generated, do not edit',
            'DEVICE=eth0',
            'BOOTPROTO=static',
            'HWADDR=00:11:22:33:44:55',
            'IPADDR=192.0.2.42',
            'NETMASK=255.255.255.0',
            'DEFROUTE=yes',
            'GATEWAY=192.0.2.1',
            'DNS1=192.0.2.2',
            'ONBOOT=yes',
            'NM_CONTROLLED=no']) + '\n')

    def test_redhat_ipv6(self):
        """Test setting public IPv6 for Red Hat networking"""
        interface = {
            'hwaddr': '00:11:22:33:44:55',
            'ipv6': [('2001:db8::42', 96)],
            'gateway6': '2001:db8::1',
            'dns': ['2001:db8::2'],
        }
        outfiles = self._run_test('redhat', public=interface)
        self.assertTrue('ifcfg-eth0' in outfiles)
        self.assertEqual(outfiles['ifcfg-eth0'], '\n'.join([
            '# Automatically generated, do not edit',
            'DEVICE=eth0',
            'BOOTPROTO=static',
            'HWADDR=00:11:22:33:44:55',
            'IPV6INIT=yes',
            'IPV6_AUTOCONF=no',
            'IPV6ADDR=2001:db8::42/96',
            'IPV6_DEFAULTGW=2001:db8::1',
            'DNS1=2001:db8::2',
            'ONBOOT=yes',
            'NM_CONTROLLED=no']) + '\n')

    def test_debian_ipv4(self):
        """Test setting public IPv4 for Debian networking"""
        interface = {
            'hwaddr': '00:11:22:33:44:55',
            'ipv4': [('192.0.2.42', '255.255.255.0')],
            'gateway4': '192.0.2.1',
            'dns': ['192.0.2.2'],
        }
        outfiles = self._run_test('debian', public=interface)
        self.assertTrue('interfaces' in outfiles)
        self.assertEqual(outfiles['interfaces'], '\n'.join([
            '# Used by ifup(8) and ifdown(8). See the interfaces(5) '
                'manpage or',
            '# /usr/share/doc/ifupdown/examples for more information.',
            '# The loopback network interface',
            'auto lo',
            'iface lo inet loopback',
            '',
            'auto eth0',
            'iface eth0 inet static',
            '    address 192.0.2.42',
            '    netmask 255.255.255.0',
            '    gateway 192.0.2.1',
            '    dns-nameservers 192.0.2.2']) + '\n')

    def test_debian_ipv6(self):
        """Test setting public IPv4 for Debian networking"""
        interface = {
            'hwaddr': '00:11:22:33:44:55',
            'ipv6': [('2001:db8::42', 96)],
            'gateway6': '2001:db8::1',
            'dns': ['2001:db8::2'],
        }
        outfiles = self._run_test('debian', public=interface)
        self.assertTrue('interfaces' in outfiles)
        self.assertEqual(outfiles['interfaces'], '\n'.join([
            '# Used by ifup(8) and ifdown(8). See the interfaces(5) '
                'manpage or',
            '# /usr/share/doc/ifupdown/examples for more information.',
            '# The loopback network interface',
            'auto lo',
            'iface lo inet loopback',
            '',
            'auto eth0',
            'iface eth0 inet6 static',
            '    address 2001:db8::42',
            '    netmask 96',
            '    gateway 2001:db8::1',
            '    dns-nameservers 2001:db8::2']) + '\n')


if __name__ == "__main__":
    agent_test.main()
