using System;
using Rackspace.Cloud.Server.Agent.Configuration;

namespace Rackspace.Cloud.Server.Agent {
    public class InvalidCommandException : Exception {
        public InvalidCommandException(string message)
            : base("Unrecognizable Command in Xen Store: " + message) {
        }
    }

    public sealed class UnsuccessfulCommandExecutionException : Exception {
        public UnsuccessfulCommandExecutionException(string message, ExecutableResult executableResult)
            : base(message)
        {
            Data.Add("result", executableResult);
        }
    }
}