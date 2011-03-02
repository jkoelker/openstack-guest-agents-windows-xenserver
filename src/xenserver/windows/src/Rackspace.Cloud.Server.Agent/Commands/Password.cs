using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent.Commands {
    /// <summary>
    /// Do not change name unless the server team is changing the command name in xen store.
    /// </summary>
    public class Password : IExecutableCommand {
        private readonly ISetPassword _setPassword;
        private readonly IAdministratorAccountNameFinder _administratorAccountNameFinder;

        public Password(ISetPassword setPassword, IAdministratorAccountNameFinder administratorAccountNameFinder) {
            _setPassword = setPassword;
            _administratorAccountNameFinder = administratorAccountNameFinder;
        }

        public ExecutableResult Execute(string value) {
            _setPassword.Execute(_administratorAccountNameFinder.Find(), value);
            return new ExecutableResult();
        }
    }
}