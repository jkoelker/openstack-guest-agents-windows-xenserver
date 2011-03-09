using System;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IServiceRestarter
    {
        void Restart(string serviceName);
    }

    public class ServiceRestarter : IServiceRestarter
    {
        private readonly IServiceStopper _serviceStopper;
        private readonly IServiceStarter _serviceStarter;
        private readonly ILogger _logger;

        public ServiceRestarter(IServiceStopper _serviceStopper, IServiceStarter _serviceStarter, ILogger logger)
        {
            this._serviceStopper = _serviceStopper;
            this._serviceStarter = _serviceStarter;
            _logger = logger;
        }

        public void Restart(string serviceName)
        {
            _logger.Log(String.Format("Restarting service '{0}' ...", serviceName));
            _serviceStopper.Stop(serviceName);
            _serviceStarter.Start(serviceName);
            _logger.Log(String.Format("Restart of service '{0}' successful.", serviceName));
        }
    }
}