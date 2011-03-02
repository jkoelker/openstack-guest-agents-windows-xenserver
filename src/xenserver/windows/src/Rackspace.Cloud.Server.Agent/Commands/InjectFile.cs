using System;
using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Actions;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Commands
{
    public class InjectFile : IExecutableCommand
    {
        private readonly IInjectFileMessageHandler _injectFileMessageHandler;
        private readonly IFileCreator _fileCreator;
        private readonly ILogger _logger;
        private FileInfo _fileInfo;

        public InjectFile(IInjectFileMessageHandler injectFileMessageHandler, IFileCreator fileCreator, ILogger logger)
        {
            _injectFileMessageHandler = injectFileMessageHandler;
            _fileCreator = fileCreator;
            _logger = logger;
        }

        public ExecutableResult Execute(string value)
        {
            try
            {
                _logger.Log(String.Format("Getting file injection information from {0}", value));
                _fileInfo = _injectFileMessageHandler.Handle(value);
                _logger.Log(String.Format("Injection File Path: {0}", _fileInfo.Path));

                _fileCreator.CreateFile(_fileInfo);    
            }
            catch(Exception e)
            {
                _logger.Log("Exception was : " + e.Message + "\nStackTrace Was: " + e.StackTrace);
                throw new UnsuccessfulCommandExecutionException("File injection failed: " + e.Message,
                    new ExecutableResult { ExitCode = "1" });
            }
            

            return new ExecutableResult();
        }
    }
}