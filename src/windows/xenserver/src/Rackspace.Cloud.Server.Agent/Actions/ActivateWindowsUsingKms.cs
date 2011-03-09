using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IActivateWindowsUsingKms {
        void Execute(string kmsServerAndPort);
    }

    public class ActivateWindowsUsingKms : IActivateWindowsUsingKms {
        private readonly IExecutableProcessQueue _executableProcessQueue;
        private readonly IOperatingSystemChecker _operatingSystemChecker;

        public ActivateWindowsUsingKms(IExecutableProcessQueue executableProcessQueue, IOperatingSystemChecker operatingSystemChecker)
        {
            _executableProcessQueue = executableProcessQueue;
            _operatingSystemChecker = operatingSystemChecker;
        }

        public void Execute(string kmsServerAndPort) {
            if(!_operatingSystemChecker.IsWindows2008) return;

            _executableProcessQueue
                .Enqueue("cscript", Constants.KmsActivationVbsPath + " /skms " + kmsServerAndPort)
                .Enqueue("cscript", Constants.KmsActivationVbsPath + " /ato")
                .Go();
        }
    }
}