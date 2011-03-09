using System.Collections.Generic;
using System.IO;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IFinalizer
    {
        void Finalize(IList<string> paths);    
    }

    public class Finalizer : IFinalizer
    {
        private readonly ILogger _logger;

        public Finalizer(ILogger logger)
        {
            _logger = logger;
        }

        public void Finalize(IList<string> paths)
        {
            _logger.Log("Cleaning temp folders and files ... ");
            foreach(var path in paths)
            {
                if (Directory.Exists(path)) Directory.Delete(path, true);
                if (File.Exists(path)) File.Delete(path);    
            }
            _logger.Log("Update Complete!");
        }
    }
}