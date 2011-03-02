using System;
using System.Configuration;

namespace Rackspace.Cloud.Server.Common.Configuration {
    public static class SvcConfiguration
    {
        public static string RemotingUriHost {
            get { return ConfigurationManager.AppSettings["RemotingUriHost"]; }
        }

        public static string RemotingUri {
            get { return ConfigurationManager.AppSettings["RemotingUri"]; }
        }

        public static int RemotingPort {
            get { return Convert.ToInt32(ConfigurationManager.AppSettings["RemotingPort"]); }
        }

        public static string AgentPath {
            get { return ConfigurationManager.AppSettings["AgentPath"]; }
        }

        public static string AgentUpdaterPath {
            get { return ConfigurationManager.AppSettings["AgentUpdaterPath"]; }
        }
        
        public static string AgentVersionUpdatesPath {
            get { return ConfigurationManager.AppSettings["AgentVersionUpdatesPath"]; }
        }
    }
}
