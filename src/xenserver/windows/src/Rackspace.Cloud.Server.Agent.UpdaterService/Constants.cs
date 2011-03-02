using Rackspace.Cloud.Server.Common.Configuration;

namespace Rackspace.Cloud.Server.Agent.UpdaterService {
    public static class Constants {
        public static readonly string AgentServiceReleasePackage = SvcConfiguration.AgentVersionUpdatesPath + "agentservice.zip";
        public static readonly string AgentServiceBackupPath = SvcConfiguration.AgentVersionUpdatesPath + "current";
        public static readonly string AgentServiceUnzipPath = SvcConfiguration.AgentVersionUpdatesPath + "agentservice";
        public const string AgentServiceName = "RackspaceCloudServersAgent";
    }
}
