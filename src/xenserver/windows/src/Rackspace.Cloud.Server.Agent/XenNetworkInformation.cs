using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent {
    public interface IXenNetworkInformation {
        Network Get();
    }

    public class XenNetworkInformation : IXenNetworkInformation {
        private readonly IXenStore _xenStore;
        private readonly string _networkKeyLocation = Constants.Combine(Constants.ReadOnlyDataConfigBase, Constants.NetworkingBase);
        public XenNetworkInformation(IXenStore xenstore) {
            _xenStore = xenstore;
        }

        public Network Get() {
            return new Network { Interfaces = GetInterfaces() };
        }

        private IDictionary<string, NetworkInterface> GetInterfaces() {
            IDictionary<string, NetworkInterface> interfaces = new Dictionary<string, NetworkInterface>();

            var macAddressesWithoutColons =
                _xenStore.Read(_networkKeyLocation);

            foreach (var macAddress in macAddressesWithoutColons) {
                var jsonData = _xenStore.ReadVmDataKey(macAddress);
                var networkInterface = new Json<NetworkInterface>().Deserialize(jsonData);
                interfaces.Add(networkInterface.mac.ToUpper(), networkInterface);
            }

            return interfaces;
        }
    }
}
