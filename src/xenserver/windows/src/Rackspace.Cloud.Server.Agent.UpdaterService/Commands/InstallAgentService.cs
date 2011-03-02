using Rackspace.Cloud.Server.Common.AgentUpdate;
using ICSharpCode.SharpZipLib.Zip;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands {
    public interface IInstallAgentService : ICommand {
    }

    public class InstallAgentService : IInstallAgentService {
        private readonly ILogger _logger;

        public InstallAgentService(ILogger logger) {
            _logger = logger;
        }

        public void Execute(AgentUpdateInfo agentUpdateInfo) {
            _logger.Log("Installing a new version of Agent ...");

            _logger.Log("Unzipping agent zip...");
            UnzipNewAgentVersion();
            _logger.Log("Copying new agent files");
            Utility.CopyFiles(Constants.AgentServiceUnzipPath, SvcConfiguration.AgentPath, _logger);
            _logger.Log("Done copying agent files.");
        }

        private void UnzipNewAgentVersion() {
            _logger.Log("Unzipping files");
            var fz = new FastZip();
            fz.ExtractZip(Constants.AgentServiceReleasePackage, Constants.AgentServiceUnzipPath, "");
            _logger.Log("Unzipping files complete");
        }
    }
}
