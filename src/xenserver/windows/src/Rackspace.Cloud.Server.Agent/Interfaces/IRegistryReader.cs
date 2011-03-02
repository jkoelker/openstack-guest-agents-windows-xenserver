using System.Collections.Generic;

namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface IRegistryReader {
        List<string> GetValuesFrom(string key);
    }
}