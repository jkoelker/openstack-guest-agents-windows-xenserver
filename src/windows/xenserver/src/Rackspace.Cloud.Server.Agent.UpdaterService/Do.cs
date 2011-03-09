using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.UpdaterService.Commands;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using StructureMap;

namespace Rackspace.Cloud.Server.Agent.UpdaterService{
    public class Do {
        private readonly AgentUpdateInfo _agentUpdateInfo;
        private readonly IList<ICommand> _listOfCommands;

        public Do(AgentUpdateInfo agentUpdateInfo) {
            _agentUpdateInfo = agentUpdateInfo;
            _listOfCommands = new List<ICommand>();
        }

        public Do Add<T>() where T : ICommand {
            _listOfCommands.Add(ObjectFactory.GetInstance<T>());
            return this;
        }

        public void Run() {
            foreach (var command in _listOfCommands)
            {
                command.Execute(_agentUpdateInfo);
            }
        }
    }
}
