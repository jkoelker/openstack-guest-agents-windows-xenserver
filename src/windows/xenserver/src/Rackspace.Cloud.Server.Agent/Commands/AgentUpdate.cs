using System;
using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Commands {
    /// <summary>
    /// Do not change name unless the server team is changing the command name in xen store.
    /// </summary>
    public class AgentUpdate : IExecutableCommand {
        private readonly IAgentUpdateMessageSender _agentUpdateMessageSender;
        private readonly IConnectionChecker _connectionChecker;
        private readonly IAgentUpdateMessageHandler _agentUpdateMessageHandler;
        private readonly ILogger _logger;

        public AgentUpdate(IAgentUpdateMessageSender agentUpdateMessageSender, IConnectionChecker connectionChecker, IAgentUpdateMessageHandler agentUpdateMessageHandler, ILogger logger) {
            _agentUpdateMessageSender = agentUpdateMessageSender;
            _connectionChecker = connectionChecker;
            _agentUpdateMessageHandler = agentUpdateMessageHandler;
            _logger = logger;
        }

        public ExecutableResult Execute(string value) {
            _connectionChecker.Check();
            _logger.Log("Agent Update value: " + value);
            var agentUpdateInfo = _agentUpdateMessageHandler.Handle(value);

            try {
                _agentUpdateMessageSender.Send(agentUpdateInfo);

                Statics.ShouldPollXenStore = false;
                return new ExecutableResult();
            }
            catch (Exception ex) {

                _logger.Log("Exception was : " + ex.Message + "\nStackTrace Was: " + ex.StackTrace);
                return new ExecutableResult { Error = new List<string> { "Update failed" }, ExitCode = "1" };
            }
        }
    }
}
