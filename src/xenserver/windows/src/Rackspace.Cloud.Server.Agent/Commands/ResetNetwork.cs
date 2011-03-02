using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent.Commands {
    /// <summary>
    /// Do not change name unless the server team is changing the command name in xen store.
    /// </summary>
    public class ResetNetwork : IExecutableCommand {
        private readonly ISetNetworkInterface _setNetworkInterface;
        private readonly IXenNetworkInformation _xenNetworkInformation;
        private readonly ISetNetworkRoutes _setNetworkRoutes;

        public ResetNetwork(ISetNetworkInterface setNetworkInterface, IXenNetworkInformation xenNetworkInformation, ISetNetworkRoutes setNetworkRoutes) {
            _setNetworkInterface = setNetworkInterface;
            _xenNetworkInformation = xenNetworkInformation;
            _setNetworkRoutes = setNetworkRoutes;
        }

        public ExecutableResult Execute(string keyValue) {
            var network = _xenNetworkInformation.Get();

            foreach (var networkinterface in network.Interfaces.Values) {
                _setNetworkInterface.Execute(networkinterface);
            }

            _setNetworkRoutes.Execute(network);

            return new ExecutableResult();
        }
    }
}