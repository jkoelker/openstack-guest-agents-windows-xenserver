using Rackspace.Cloud.Server.Common.AgentUpdate;

namespace Rackspace.Cloud.Server.Common.Communication {
    public interface IAgentUpdater {
        void DoUpdate(AgentUpdateInfo agentUpdateInfo);
    }
}