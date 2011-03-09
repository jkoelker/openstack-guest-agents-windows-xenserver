using System;
using System.ServiceProcess;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IServiceStarter
    {
        void Start(string serviceName);
    }

    public class ServiceStarter : IServiceStarter
    {
        private readonly ILogger _logger;

        public ServiceStarter(ILogger logger)
        {
            _logger = logger;
        }

        public void Start(string serviceName)
        {
            _logger.Log(String.Format("Starting '{0}' Service ...", serviceName));
            var serviceController = new ServiceController(serviceName);
            if (serviceController.Status == ServiceControllerStatus.Running)
            {
                _logger.Log(String.Format("'{0}' service already started.", serviceName));
                return;
            }

            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running);

            serviceController.Close();
            _logger.Log(String.Format("Service '{0}' started and now running ...", serviceName));
        }
    }
}