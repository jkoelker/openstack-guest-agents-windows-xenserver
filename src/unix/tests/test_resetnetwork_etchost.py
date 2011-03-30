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
resetnetwork /etc/hosts tester
"""

import os
import unittest
from cStringIO import StringIO

import commands.network


class TestEtcHostUpdates(unittest.TestCase):

    def _setup_input(self, *args):
        infile = StringIO()
        for entry in args:
            print >> infile, '%s\t%s' % (entry[0], ' '.join(entry[1:]))
        infile.seek(0)
        return infile

    def _run_test(self, infile, hostname, *args):
        outfile = commands.network._update_etc_hosts(infile, set(args),
            hostname)
        outfile.seek(0)
        return outfile

    def test_empty(self):
        """Test update empty /etc/hosts"""
        infile = self._setup_input()
        outfile = self._run_test(infile, 'example', '192.0.2.1')
        self.assertEqual(outfile.read(), '192.0.2.1\texample\n')

    def test_add(self):
        """Test update one-line /etc/hosts with new entry"""
        infile = self._setup_input(('192.0.2.2', 'other'))
        outfile = self._run_test(infile, 'example', '192.0.2.1')
        self.assertEqual(outfile.read(),
            '192.0.2.2\tother\n' +
            '192.0.2.1\texample\n')

    def test_update_one_hostname(self):
        """Test update one-line /etc/hosts with one hostname"""
        infile = self._setup_input(('192.0.2.1', 'oldname'))
        outfile = self._run_test(infile, 'example', '192.0.2.1')
        self.assertEqual(outfile.read(),
            '# 192.0.2.1\toldname\t# Removed by nova-agent\n' +
            '192.0.2.1\texample\n')

    def test_update_two_hostnames(self):
        """Test update one-line /etc/hosts with two hostnames"""
        infile = self._setup_input(('192.0.2.1', 'oldname',
            'oldname.example.com'))
        outfile = self._run_test(infile, 'example', '192.0.2.1')
        self.assertEqual(outfile.read(),
            '# 192.0.2.1\toldname oldname.example.com\t' +
                '# Removed by nova-agent\n' +
            '192.0.2.1\texample oldname.example.com\n')

    def test_update_two_entries(self):
        """Test update two-line /etc/hosts with one hostname"""
        infile = self._setup_input(('192.0.2.1', 'oldname'),
            ('192.0.2.2', 'other'))
        outfile = self._run_test(infile, 'example', '192.0.2.1')
        self.assertEqual(outfile.read(),
            '# 192.0.2.1\toldname\t# Removed by nova-agent\n' +
            '192.0.2.1\texample\n' +
            '192.0.2.2\tother\n')

    def test_update_comment(self):
        """Test update one-line /etc/hosts with trailing comment"""
        infile = self._setup_input(('192.0.2.1', 'oldname', '# comment'))
        outfile = self._run_test(infile, 'example', '192.0.2.1')
        self.assertEqual(outfile.read(),
            '# 192.0.2.1\toldname # comment\t# Removed by nova-agent\n' +
            '192.0.2.1\texample\t# comment\n')


if __name__ == "__main__":
    agent_test.main()
