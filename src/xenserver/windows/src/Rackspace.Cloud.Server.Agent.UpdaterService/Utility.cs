using System;
using System.IO;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService {
    public class Utility {
        public static void CopyFiles(string fromPath, string toPath, ILogger logger) {
            foreach (var file in Directory.GetFiles(fromPath)) {
                if (file.Contains(".zip")) continue;
//                logger.Log(String.Format("Copying file {0} to {1}", file, Path.Combine(toPath,Path.GetFileName(file))));
                File.Copy(file, Path.Combine(toPath, Path.GetFileName(file)), true);
            }
        }
    }
}