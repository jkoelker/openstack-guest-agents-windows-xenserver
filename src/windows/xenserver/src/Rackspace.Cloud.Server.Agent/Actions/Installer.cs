using System;
using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IInstaller
    {
        void Install(Dictionary<string, string> commands);
    }
    public class Installer : IInstaller
    {
        private readonly IExecutableProcessQueue _executableProcessQueue;
        private readonly ILogger _logger;

        public Installer(IExecutableProcessQueue executableProcessQueue, ILogger logger)
        {
            _executableProcessQueue = executableProcessQueue;
            _logger = logger;
        }

        public void Install(Dictionary<string, string> commands)
        {
            _logger.Log("Installing...");
            foreach(var key in commands.Keys)
            {
                _logger.Log(String.Format("Running command: '{0} {1}'", key, commands[key]));
                _executableProcessQueue
                    .Enqueue(key, commands[key])
                    .Go();    
            }
            _logger.Log("Install complete...");
        }
    }
}