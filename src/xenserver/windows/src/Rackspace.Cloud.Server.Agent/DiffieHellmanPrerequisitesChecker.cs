using System;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent {
    public class DiffieHellmanPrerequisitesChecker : IDiffieHellmanPrerequisitesChecker {
        public bool ArePresent {
            get {
                return !String.IsNullOrEmpty(Statics.DiffieHellmanCollaboratorKey)
                       && Statics.DiffieHellman != null;
            }
        }
    }
}