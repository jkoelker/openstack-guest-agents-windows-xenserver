using System;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Security;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface ISetPassword {
        void Execute(string user, string encryptedPassword);
    }

    public class SetPassword : ISetPassword {
        private readonly IExecutableProcessQueue _executableProcessQueue;
        private readonly IDiffieHellmanPrerequisitesChecker _prerequisitesChecker;

        public SetPassword(IExecutableProcessQueue executableProcessQueue, IDiffieHellmanPrerequisitesChecker prerequisitesChecker) {
            _executableProcessQueue = executableProcessQueue;
            _prerequisitesChecker = prerequisitesChecker;
        }

        public void Execute(string user, string encryptedPassword) {
            if(!_prerequisitesChecker.ArePresent) throw new UnsuccessfulCommandExecutionException("Key init was not called prior to Set Password command", new ExecutableResult {ExitCode = "1"});

            var decryptionKey = Statics.DiffieHellman.DecryptKeyExchange(Statics.DiffieHellmanCollaboratorKey);

            var password = Encryption.Decrypt(Convert.FromBase64String(encryptedPassword), decryptionKey, new byte[0]);
            _executableProcessQueue.Enqueue("net", "user " + user + " " + password).Go();
            
            Statics.DiffieHellman = null;
            Statics.DiffieHellmanCollaboratorKey = null;
        }
    }
}