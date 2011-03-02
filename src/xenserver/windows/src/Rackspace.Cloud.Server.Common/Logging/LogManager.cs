using log4net;
using log4net.Config;

namespace Rackspace.Cloud.Server.Common.Logging
{
    public static class LogManager {
        private static ILog _logmanager;

        static LogManager()
        {
            ShouldBeLogging = true;
        }

        public static ILog Instance {
            get {
                if (_logmanager == null) {
                    XmlConfigurator.Configure();
                    _logmanager = log4net.LogManager.GetLogger("AgentService");
                }

                return _logmanager;
            }
        }

        public static bool ShouldBeLogging { get; set; }
    }
}