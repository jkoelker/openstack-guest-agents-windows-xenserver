using System.ServiceProcess;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands {
    public interface IStartAgentService : ICommand {
    }

    public class StartAgentService : IStartAgentService {
        private readonly ILogger _logger;

        public StartAgentService(ILogger logger) {
            _logger = logger;
        }

        public void Execute(AgentUpdateInfo agentUpdateInfo) {
            _logger.Log("Starting Agent Service ...");
            var serviceController = new ServiceController(Constants.AgentServiceName);
            if (serviceController.Status == ServiceControllerStatus.Running) {
                _logger.Log("Agent service already started.");
                return;
            }

            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running);

            serviceController.Close();
            _logger.Log("Agent Service started and now running ...");
        }
    }
}
