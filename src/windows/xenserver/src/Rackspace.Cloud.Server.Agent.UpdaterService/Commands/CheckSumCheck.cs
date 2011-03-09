using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Rackspace.Cloud.Server.Common.AgentUpdate;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.UpdaterService.Commands {
    public interface ICheckSumCheck : ICommand {
    }

    public class CheckSumCheck : ICheckSumCheck {
        private readonly ILogger _logger;

        public CheckSumCheck(ILogger logger) {
            _logger = logger;
        }

        public void Execute(AgentUpdateInfo agentUpdateInfo) {

            var fileName = Constants.AgentServiceReleasePackage;
            if (!File.Exists(fileName)) {
                throw new ArgumentException(string.Format("Filename '{0}' does not exist.", fileName));
            }

            _logger.Log("Verifying checksum of downloaded agent version...");

            using (var fstream = new FileStream(fileName, FileMode.Open)) {
                var hash = new MD5CryptoServiceProvider().ComputeHash(fstream);

                var result = new StringBuilder();
                foreach (var b in hash)
                    result.AppendFormat("{0:x2}", b);

                var checksum = result.ToString().ToUpper();

                if (checksum != agentUpdateInfo.signature.ToUpper()) {
                    throw new BadChecksumException(String.Format("Checksum verification failed. Incorrect checksum: {0}", checksum));
                }
            }

            _logger.Log("Checksum Validated.");
        }
    }
}
