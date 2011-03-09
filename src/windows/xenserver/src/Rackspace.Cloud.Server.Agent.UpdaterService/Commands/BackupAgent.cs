using System.IO;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands
{
    public interface IBackupAgent : ICommand {
    }

    public class BackupAgent : IBackupAgent {
        private readonly ILogger _logger;

        public BackupAgent(ILogger logger) {
            _logger = logger;
        }

        public void Execute(AgentUpdateInfo agentUpdateInfo) {
            Setup(Constants.AgentServiceBackupPath);
            Utility.CopyFiles(SvcConfiguration.AgentPath, Constants.AgentServiceBackupPath, _logger);
        }

        private void Setup(string toPath) {
            if (Directory.Exists(toPath)) Directory.Delete(toPath, true);
            Directory.CreateDirectory(toPath);
        }
    }
}