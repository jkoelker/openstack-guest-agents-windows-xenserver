using System.Linq;
using System.Reflection;
using Rackspace.Cloud.Server.Agent.Interfaces;
using StructureMap;

namespace Rackspace.Cloud.Server.Agent {
    public class CommandFactory : ICommandFactory {
        public IExecutableCommand CreateCommand(string dataValue) {
            var key = dataValue.ToLower();
            var matchingCommand = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.Namespace == "Rackspace.Cloud.Server.Agent.Commands")
                .SingleOrDefault(x => x.Name.ToLower() == key);

            if (matchingCommand == null) throw new InvalidCommandException(dataValue);

            return ObjectFactory.GetNamedInstance<IExecutableCommand>(key);
        }
    }
}