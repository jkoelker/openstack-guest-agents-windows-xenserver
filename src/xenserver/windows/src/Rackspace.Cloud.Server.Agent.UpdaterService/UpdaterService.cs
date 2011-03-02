using System;
using System.ServiceProcess;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService {
    public partial class UpdaterService : ServiceBase {
        private readonly HostUpdater _hostUpdater;
        private readonly Logger _logger;

        public UpdaterService() {
            InitializeComponent();
            _logger = new Logger();
            _hostUpdater = new HostUpdater(_logger);
        }

        protected override void OnStart(string[] args) {
            try {
                _hostUpdater.OnStart();
            } catch (Exception ex) {
                _logger.Log("Exception was : " + ex.Message + "\nStackTrace Was: " + ex.StackTrace);
            }
        }

        protected override void OnStop() {
            _hostUpdater.OnStop();
        }
    }
}
