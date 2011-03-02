using System.Collections.Generic;

namespace Rackspace.Cloud.Server.Agent.Configuration {
    public class Network {
        public Network() {
            Interfaces = new Dictionary<string, NetworkInterface>();
        }

        public IDictionary<string, NetworkInterface> Interfaces { get; set; }
    }
}