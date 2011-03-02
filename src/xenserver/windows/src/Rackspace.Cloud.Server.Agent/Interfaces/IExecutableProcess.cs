using Rackspace.Cloud.Server.Agent.Configuration;

namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface IExecutableProcess {
        ExecutableResult Run(string fileName, string arguments);
    }
}