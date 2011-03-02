using System;

namespace Rackspace.Cloud.Server.Agent
{
    [Serializable]
    public class FileInfo
    {
        public string Path { get; set; }
        public string Content { get; set; }
    }
}