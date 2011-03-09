using System.Collections.Generic;
using System.Management;

namespace Rackspace.Cloud.Server.Agent.WMI {
    public interface IWmiMacNetworkNameGetter {
        IDictionary<string, string> Get();
    }

    public class WmiMacNetworkNameGetter : IWmiMacNetworkNameGetter {
        public IDictionary<string, string> Get() {
            var macAndNetworkName = new Dictionary<string, string>();
            var query = new ObjectQuery("select netconnectionid, MACAddress from win32_NetworkAdapter where netconnectionid is not null");

            var scope = new ManagementScope();
            var searcher = new ManagementObjectSearcher(scope, query);
            var obj = searcher.Get();

            foreach (ManagementObject collection in obj) {
                var macAddress = collection["MACAddress"] == null ? "" : collection["MACAddress"].ToString();
                macAndNetworkName.Add(collection["netconnectionid"].ToString(), macAddress);
            }

            return macAndNetworkName;
        }
    }
}