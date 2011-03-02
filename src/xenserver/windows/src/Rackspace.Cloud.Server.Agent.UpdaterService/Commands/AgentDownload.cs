using System.IO;
using System.Net;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands {
    public interface IAgentDownload : ICommand {
    }

    public class AgentDownload : IAgentDownload {
        private readonly ILogger _logger;

        public AgentDownload(ILogger logger) {
            _logger = logger;
        }

        public void Execute(AgentUpdateInfo agentUpdateInfo) {
            _logger.Log("Downloading Agent ...");

            if (!Directory.Exists(SvcConfiguration.AgentVersionUpdatesPath))
                Directory.CreateDirectory(SvcConfiguration.AgentVersionUpdatesPath);

            var webClient = new WebClient();
            webClient.DownloadFile(agentUpdateInfo.url, Constants.AgentServiceReleasePackage);

            _logger.Log("Agent downloaded.");
        }
    }
}
