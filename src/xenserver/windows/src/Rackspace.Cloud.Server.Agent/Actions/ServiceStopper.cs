using System;
using System.ServiceProcess;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IServiceStopper
    {
        void Stop(string serviceName);
    }

    public class ServiceStopper : IServiceStopper
    {
        private readonly ILogger _logger;

        public ServiceStopper(ILogger logger)
        {
            _logger = logger;
        }

        public void Stop(string serviceName)
        {
            _logger.Log(String.Format("Stopping Service '{0}' ...", serviceName));

            var serviceController = new ServiceController(serviceName);
            if (serviceController.Status == ServiceControllerStatus.Stopped)
            {
                _logger.Log(String.Format("Service '{0}' already stopped.", serviceName));
                return;
            }

            if (!serviceController.CanStop)
                throw new ApplicationException(
                    String.Format("Service '{0}' can't be stop at this time, please try again later", serviceName));

            serviceController.Stop();
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);

            serviceController.Close();

            _logger.Log(String.Format("Service '{0}' successfully stopped.", serviceName));
        }
    }
}