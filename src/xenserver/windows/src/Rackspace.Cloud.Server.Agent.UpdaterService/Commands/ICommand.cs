using Rackspace.Cloud.Server.Common.AgentUpdate;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands {
    public interface ICommand {
        void Execute(AgentUpdateInfo agentUpdateInfo);
    }
}