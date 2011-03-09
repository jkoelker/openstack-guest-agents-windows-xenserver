using System.Collections.Generic;

namespace Rackspace.Cloud.Server.Agent.Configuration
{
    public class NameServer
    {
        public NameServer()
        {
            ResolverIPs = new List<string>();
        }
        public string DefaultDomain { get; set; }
        public IList<string> ResolverIPs { get; set; }
    }
}