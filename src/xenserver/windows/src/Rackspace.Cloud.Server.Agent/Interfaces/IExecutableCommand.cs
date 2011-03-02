using Rackspace.Cloud.Server.Agent.Configuration;

namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface IExecutableCommand {
        ExecutableResult Execute(string value);
    }
}