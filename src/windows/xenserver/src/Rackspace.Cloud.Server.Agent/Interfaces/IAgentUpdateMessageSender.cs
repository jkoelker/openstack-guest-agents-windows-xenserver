using Rackspace.Cloud.Server.Common.AgentUpdate;

namespace Rackspace.Cloud.Server.Agent.Interfaces
{
    public interface IAgentUpdateMessageSender {
        void Send(AgentUpdateInfo agentUpdateInfo);
    }
}