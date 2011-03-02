using Rackspace.Cloud.Server.DiffieHellman;

namespace Rackspace.Cloud.Server.Agent.Utilities {
    public class Statics {
        public static bool ShouldPollXenStore = true;
        public static IDiffieHellman DiffieHellman;
        public static string DiffieHellmanCollaboratorKey;
    }
}