using System.IO;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands
{
    public interface IFinalize : ICommand {
    }

    public class Finalize : IFinalize {
        private readonly ILogger _logger;

        public Finalize(ILogger logger) {
            _logger = logger;
        }

        public void Execute(AgentUpdateInfo agentUpdateInfo) {
            _logger.Log("Cleaning temp files/folders ... ");
            if (Directory.Exists(Constants.AgentServiceBackupPath)) Directory.Delete(Constants.AgentServiceBackupPath, true);
            if (File.Exists(Constants.AgentServiceReleasePackage)) File.Delete(Constants.AgentServiceReleasePackage);
            if (Directory.Exists(Constants.AgentServiceUnzipPath)) Directory.Delete(Constants.AgentServiceUnzipPath, true);
            _logger.Log("Cleaning Complete ...");
        }
    }
}
