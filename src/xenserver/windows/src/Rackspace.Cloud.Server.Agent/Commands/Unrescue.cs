using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent.Commands
{
    public class Unrescue : IExecutableCommand
    {
        private readonly IExecutableProcessQueue _exec;
        private readonly IOperatingSystemChecker _operatingSystemChecker;
        private readonly ISystemInformation _systemInformation;

        public Unrescue(IExecutableProcessQueue exec, IOperatingSystemChecker operatingSystemChecker, ISystemInformation systemInformation)
        {
            _exec = exec;
            _operatingSystemChecker = operatingSystemChecker;
            _systemInformation = systemInformation;
        }

        public ExecutableResult Execute(string value) {
            if(!_operatingSystemChecker.IsWindows2008) return new ExecutableResult();

            var bcdDataStoreDrive = _systemInformation.getDriveWithBootConfigurationDataStore();
            var originalDriveLetterWithWindowsFolder = _systemInformation
                .getDriveLetterWithOriginalWindowsFolder();

            _exec
                .Enqueue("bcdedit", "/store " + bcdDataStoreDrive[SystemInformation.DRIVE_KEY] + "boot\\bcd /set " + bcdDataStoreDrive[SystemInformation.UNIQUE_IDENTIFIER_KEY] + " osdevice partition=" + originalDriveLetterWithWindowsFolder)
                .Enqueue("bcdedit", "/store " + bcdDataStoreDrive[SystemInformation.DRIVE_KEY] + "boot\\bcd /set " + bcdDataStoreDrive[SystemInformation.UNIQUE_IDENTIFIER_KEY] + " device partition=" + originalDriveLetterWithWindowsFolder)
                .Enqueue("bcdedit", "/store " + bcdDataStoreDrive[SystemInformation.DRIVE_KEY] + "boot\\bcd /set {bootmgr} device partition=" + originalDriveLetterWithWindowsFolder)
                .Enqueue("bcdedit", "/store " + bcdDataStoreDrive[SystemInformation.DRIVE_KEY] + "boot\\bcd /set {memdiag} device partition=" + originalDriveLetterWithWindowsFolder)
                .Enqueue("bcdedit", "/store " + bcdDataStoreDrive[SystemInformation.DRIVE_KEY] + "boot\\bcd /set {ntldr} device partition=" + originalDriveLetterWithWindowsFolder, _operatingSystemChecker.IsWindows2008SP2)
                .Go();

            return new ExecutableResult();
        }
    }
}