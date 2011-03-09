using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent.Commands
{
    public class KmsActivate : IExecutableCommand
    {
        private readonly IActivateWindowsUsingKms _activateWindowsUsingKms;

        public KmsActivate(IActivateWindowsUsingKms activateWindowsUsingKms)
        {
            _activateWindowsUsingKms = activateWindowsUsingKms;
        }

        public ExecutableResult Execute(string value)
        {
            _activateWindowsUsingKms.Execute(value);
            return new ExecutableResult();
        }
    }
}
