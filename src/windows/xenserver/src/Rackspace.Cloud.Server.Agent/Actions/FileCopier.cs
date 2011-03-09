using System;
using System.IO;
using System.Linq;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IFileCopier
    {
        void CopyFiles(string fromPath, string toPath, ILogger logger);
    }

    public class FileCopier : IFileCopier
    {
        public void CopyFiles(string fromPath, string toPath, ILogger logger)
        {
            if(!Directory.Exists(fromPath))
                throw new DirectoryNotFoundException(String.Format("{0} does not exist", fromPath));

            foreach (var file in Directory.GetFiles(fromPath).Where(file => !file.Contains(".zip")))
            {
                //logger.Log(String.Format("Copying file {0} to {1}", file, Path.Combine(toPath,Path.GetFileName(file))));
                File.Copy(file, Path.Combine(toPath, Path.GetFileName(file)), true);
            }
        }
    }
}
