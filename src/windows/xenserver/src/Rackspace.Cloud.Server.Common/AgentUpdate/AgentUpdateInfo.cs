using System;

namespace Rackspace.Cloud.Server.Common.AgentUpdate
{
    [Serializable]
    public class AgentUpdateInfo {
        public string url { get; set; }
        public string signature { get; set; }
    }
}