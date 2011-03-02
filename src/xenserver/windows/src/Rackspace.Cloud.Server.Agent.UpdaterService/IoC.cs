using Rackspace.Cloud.Server.Agent.UpdaterService.Commands;
using Rackspace.Cloud.Server.Common.Logging;
using StructureMap;

namespace Rackspace.Cloud.Server.Agent.UpdaterService {
    public static class IoC {
        public static void Register() {
            StructureMapConfiguration.BuildInstancesOf<ILogger>().TheDefaultIsConcreteType<Logger>();
            StructureMapConfiguration.BuildInstancesOf<IBackupAgent>().TheDefaultIsConcreteType<BackupAgent>();
            StructureMapConfiguration.BuildInstancesOf<IAgentDownload>().TheDefaultIsConcreteType<AgentDownload>();
            StructureMapConfiguration.BuildInstancesOf<ICheckSumCheck>().TheDefaultIsConcreteType<CheckSumCheck>();
            StructureMapConfiguration.BuildInstancesOf<IStopAgentService>().TheDefaultIsConcreteType<StopAgentService>();
            StructureMapConfiguration.BuildInstancesOf<IStartAgentService>().TheDefaultIsConcreteType<StartAgentService>();
            StructureMapConfiguration.BuildInstancesOf<IInstallAgentService>().TheDefaultIsConcreteType<InstallAgentService>();
            StructureMapConfiguration.BuildInstancesOf<IFinalize>().TheDefaultIsConcreteType<Finalize>();
        }
    }
}