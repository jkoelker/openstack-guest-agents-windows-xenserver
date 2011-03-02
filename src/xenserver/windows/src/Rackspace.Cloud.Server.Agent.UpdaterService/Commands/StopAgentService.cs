using System;
using System.ServiceProcess;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands {
    public interface IStopAgentService : ICommand {
    }

    public class StopAgentService : IStopAgentService {
        private readonly ILogger _logger;

        public StopAgentService(ILogger logger) {
            _logger = logger;
        }

        public void Execute(AgentUpdateInfo agentUpdateInfo) {
            _logger.Log("Stopping Agent Service ...");

            var serviceController = new ServiceController(Constants.AgentServiceName);
            if(serviceController.Status == ServiceControllerStatus.Stopped) {
                _logger.Log("Agent Service already stopped.");
                return;
            }

            if(!serviceController.CanStop) throw new ApplicationException("Service {0} can't be stop at this time, please try again later");
            
            serviceController.Stop();
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);

            serviceController.Close();

            _logger.Log("Agent Service successfully stopped.");
        }
    }
}
