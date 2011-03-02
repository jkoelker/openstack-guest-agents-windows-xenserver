using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Actions;

namespace Rackspace.Cloud.Server.Agent.Commands {
    /// <summary>
    /// Do not change name unless the server team is changing the command name in xen store.
    /// </summary>
    public class Version : IExecutableCommand {
        private readonly IVersionChecker _versionChecker;
        public const string XENTOOLS = "xentools";
        public const string AGENT = "agent";
        public const string AGENT_UPDATER = "updater";
        public const string XENTOOLS_PATH = @"C:\Program Files\Citrix\XenTools\xenservice.exe";
        public const string AGENT_PATH = @"C:\Program Files\Rackspace\Cloud Servers\Agent\Rackspace.Cloud.Server.Agent.dll";
        public const string AGENT_UPDATER_PATH = @"C:\Program Files\Rackspace\Cloud Servers\AgentUpdater\Rackspace.Cloud.Server.Agent.UpdaterService.exe";

        public Version(IVersionChecker versionChecker)
        {
            _versionChecker = versionChecker;
        }

        public ExecutableResult Execute(string value)
        {
            var path = "";
            if (value == XENTOOLS) path = XENTOOLS_PATH;
            if (value == AGENT) path = AGENT_PATH;
            if (value == AGENT_UPDATER) path = AGENT_UPDATER_PATH;

            return new ExecutableResult
                       {
                           Output = new[] {_versionChecker.Check(path)}
                       };
        }
    }
}
