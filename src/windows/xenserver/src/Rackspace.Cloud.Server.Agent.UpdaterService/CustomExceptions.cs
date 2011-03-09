using System;

namespace Rackspace.Cloud.Server.Agent.UpdaterService {
    public class BadChecksumException : Exception {
        public BadChecksumException(string message) : base(message) { }
    }
}
