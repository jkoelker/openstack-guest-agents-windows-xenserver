using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IVersionChecker
    {
        string Check(string filePath);
    }

    public class VersionChecker : IVersionChecker
    {
        private readonly ILogger _logger;

        public VersionChecker(ILogger logger)
        {
            _logger = logger;
        }

        public string Check(string filePath)
        {
            _logger.Log(string.Format("Checking version of '{0}'", filePath));
            string result;
            try
            {
                result = FileVersionInfo.GetVersionInfo(filePath).ProductVersion;
            }
            catch(FileNotFoundException e)
            {
                _logger.Log(String.Format("Exception was : {0}\nStackTrace Was: {1}", e.Message, e.StackTrace));
                result = String.Format("File {0} not found", filePath);
            }
            catch(Exception e)
            {
                _logger.Log(String.Format("Exception was : {0}\nStackTrace Was: {1}", e.Message, e.StackTrace));
                result = String.Format("Error trying to check version of {0}", filePath);
            }
            return result;
            
        }
    }
}