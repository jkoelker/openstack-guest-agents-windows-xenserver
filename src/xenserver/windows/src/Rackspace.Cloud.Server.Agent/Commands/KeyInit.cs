using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.DiffieHellman;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent.Commands {
    /// <summary>
    /// Do not change name unless the server team is changing the command name in xen store.
    /// </summary>
    public class KeyInit : IExecutableCommand {
        private readonly IDiffieHellman _diffieHellman;

        public KeyInit(IDiffieHellman diffieHellman) {
            _diffieHellman = diffieHellman;
        }

        public ExecutableResult Execute(string value) {
            Statics.DiffieHellmanCollaboratorKey = value;
            Statics.DiffieHellman = _diffieHellman;

            return new ExecutableResult
                       {
                           ExitCode = Constants.SuccessfulKeyInit,
                           Output = _diffieHellman.CreateKeyExchange().SplitOnNewLine()
                       };
        }
    }
}