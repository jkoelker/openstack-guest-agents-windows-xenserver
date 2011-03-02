using System.Collections.Generic;

namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface IExecutableProcessCommandPatternSubsitution {
        IDictionary<string, string> GetSubsitutions();
    }
}