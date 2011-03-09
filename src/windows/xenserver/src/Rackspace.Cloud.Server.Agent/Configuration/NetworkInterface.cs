namespace Rackspace.Cloud.Server.Agent.Configuration {
    public class NetworkInterface {
        public string mac { get; set; }
        public string[] dns { set; get; }
        public string label { get; set; }
        public IpTuple[] ips { get; set; }
        public string gateway { get; set; }
        public NetworkRoute[] routes { get; set; }
    }
}