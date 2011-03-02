using System;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Communication;
using Rackspace.Cloud.Server.Common.Configuration;

namespace Rackspace.Cloud.Server.Agent {
    public class AgentUpdateMessageSender : IAgentUpdateMessageSender {
        public void Send(AgentUpdateInfo agentUpdateInfo) {
            IAgentUpdater agentUpdater;
            try {
                ConnectToRemotingHost(out agentUpdater);
                agentUpdater.DoUpdate(agentUpdateInfo);
            }
            catch (Exception ex) {
                throw new UnsuccessfulCommandExecutionException(
                    String.Format("UPDATE FAILED: {0}", ex.Message),
                    new ExecutableResult { ExitCode = "1" });
            }
        }

        private void ConnectToRemotingHost(out IAgentUpdater agentUpdater) {
            agentUpdater = (IAgentUpdater)Activator.GetObject(typeof(IAgentUpdater), BuildRemotingUri());
        }

        private string BuildRemotingUri()
        {
            return String.Format("tcp://{0}:{1}/{2}",
                                 SvcConfiguration.RemotingUriHost,
                                 SvcConfiguration.RemotingPort,
                                 SvcConfiguration.RemotingUri);
        }
    }
}
