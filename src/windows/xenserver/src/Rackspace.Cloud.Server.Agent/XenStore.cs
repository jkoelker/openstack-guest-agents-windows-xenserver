using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent {
    public class XenStore : IXenStore {
        private readonly IExecutableProcess _executableProcess;

        public XenStore(IExecutableProcess executableProcess) {
            _executableProcess = executableProcess;
        }

        public IEnumerable<string> Read(string key) {
            return _executableProcess.Run(Constants.XenClientPath, "dir " + key).Output;
        }

        public IList<Command> GetCommands() {
            var messageKeysAsUuids = Read(Constants.WritableDataHostBase).ValidateAndClean();
            IList<Command> commands = new List<Command>();

            foreach (var messageKey in messageKeysAsUuids) {
                var command = new Json<Command>().Deserialize(ReadKey(messageKey));
                command.key = messageKey;
                commands.Add(command);
            }

            return commands;
        }

        public string ReadKey(string key) {
            var result = _executableProcess.Run(Constants.XenClientPath, "read " + Constants.Combine(Constants.WritableDataHostBase, key));
            return result.Output.First();
        }

        public string ReadVmDataKey(string key) {
            var result = _executableProcess.Run(Constants.XenClientPath, "read " + Constants.Combine(Constants.ReadOnlyDataConfigBase, Constants.NetworkingBase, key));
            return result.Output.First();
        }

        public void Write(string key, string value) {
            _executableProcess.Run(Constants.XenClientPath, "write " + Constants.Combine(Constants.WritableDataGuestBase, key) + " " + value.EscapeQuotesForXenClientWrite());
        }

        public void Remove(string key) {
            _executableProcess.Run(Constants.XenClientPath, "remove " + Constants.Combine(Constants.WritableDataHostBase, key));
        }
    }
}