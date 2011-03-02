using System;

namespace Rackspace.Cloud.Server.Agent
{
    public interface IOperatingSystemChecker
    {
        bool IsWindows2008 { get; }
        bool IsWindows2008SP2 { get; }
        bool IsWindows2008R2 { get; }
    }

    public class OperatingSystemChecker : IOperatingSystemChecker
    {
        private const int WINDOWS_2008_MAJOR_BUILD_NUMBER = 6;
        private const int WINDOWS_2008_R2_MINOR_BUILD_NUMBER = 1;
        private const int WINDOWS_2008_SP2_MINOR_BUILD_NUMBER = 0;

        public bool IsWindows2008
        {
            get { return IsWindows2008R2 || IsWindows2008SP2; }
        }

        public bool IsWindows2008SP2
        {
            get
            {
                return Environment.OSVersion.Version.Major == WINDOWS_2008_MAJOR_BUILD_NUMBER &&
                    Environment.OSVersion.Version.Minor == WINDOWS_2008_SP2_MINOR_BUILD_NUMBER;
            }
        }

        public bool IsWindows2008R2
        {
            get
            {
                return Environment.OSVersion.Version.Major == WINDOWS_2008_MAJOR_BUILD_NUMBER &&
                     Environment.OSVersion.Version.Minor == WINDOWS_2008_R2_MINOR_BUILD_NUMBER;
            }
        }
    }
}