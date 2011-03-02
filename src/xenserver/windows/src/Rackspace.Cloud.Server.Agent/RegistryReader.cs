using System.Collections.Generic;
using Microsoft.Win32;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent {
    public class RegistryReader : IRegistryReader {
        public List<string> GetValuesFrom(string key) {
            var masterKey = Registry.LocalMachine.OpenSubKey(key);
            
            if (masterKey == null) return new List<string>();

            var names = masterKey.GetValueNames();
            var result = new List<string>();
            foreach (var name in names) {
                result.Add(name);
            }
            return result;
        }
    }
}