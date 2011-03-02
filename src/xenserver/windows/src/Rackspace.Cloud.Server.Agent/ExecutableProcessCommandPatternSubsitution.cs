using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent {
    public class ExecutableProcessCommandPatternSubsitution : IExecutableProcessCommandPatternSubsitution {
        public IDictionary<string, string> GetSubsitutions() {
            return new Dictionary<string, string>
                       {
                           {"(net user (.*)\\s)+(\\S+)","$1*****"}
                       };
        }
    }
}
