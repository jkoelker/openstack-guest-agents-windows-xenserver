using System.ServiceProcess;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Service {
    partial class AgentService : ServiceBase {
        readonly ServerClass _serverClass;

        public AgentService() {
            InitializeComponent();
            _serverClass = new ServerClass(new Logger());
        }

        protected override void OnStart(string[] args) {
            _serverClass.Onstart();
        }

        protected override void OnStop() {
            _serverClass.Onstop();
        }
    }
}