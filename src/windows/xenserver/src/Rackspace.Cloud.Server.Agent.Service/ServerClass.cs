using System;
using System.Reflection;
using System.Timers;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Common.Logging;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Rackspace.Cloud.Server.Agent.Service {
    public class ServerClass {
        private readonly ILogger _logger;
        private ITimer _timer;

        public ServerClass(ILogger logger) {
            _logger = logger;
        }

        public void Onstart() {
            _logger.Log("Agent Service Starting ...");
            _logger.Log("Agent Version: " + Assembly.GetExecutingAssembly().GetName().Version);

            const int TIMER_INTERVAL_IS_SIX_SECONDS = 6000;

            _timer = new ProdTimer { Interval = TIMER_INTERVAL_IS_SIX_SECONDS };
            _timer.Elapsed(TimerElapsed);
            _timer.Enabled = true;

            StructureMapConfiguration.UseDefaultStructureMapConfigFile = false;
            StructureMapConfiguration.BuildInstancesOf<ITimer>().TheDefaultIs(Registry.Object(_timer));
            IoC.Register();
        }

        public void Onstop() {
            LogManager.ShouldBeLogging = true;
            _logger.Log("Agent Service Stopping ...");
            _timer.Enabled = false;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) {
            try {
                ObjectFactory.GetInstance<ServiceWork>().Do();
            } catch (Exception ex) {
                _logger.Log("Exception was : " + ex.Message + "\nStackTrace Was: " + ex.StackTrace);
            }
        }
    }
}