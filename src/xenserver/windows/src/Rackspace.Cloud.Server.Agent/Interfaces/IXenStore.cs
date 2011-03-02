using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Configuration;

namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface IXenStore {
        IList<Command> GetCommands();
        void Write(string key, string value);
        void Remove(string key);
        IEnumerable<string> Read(string keylocation);
        string ReadKey(string s);
        string ReadVmDataKey(string key);
    }
}