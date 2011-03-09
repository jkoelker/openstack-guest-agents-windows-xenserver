using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent.Commands {
    /// <summary>
    /// Do not change name unless the server team is changing the command name in xen store.
    /// </summary>
    public class Ready : IExecutableCommand {
        public ExecutableResult Execute(string value) {
            return new ExecutableResult();
        }
    }
}
