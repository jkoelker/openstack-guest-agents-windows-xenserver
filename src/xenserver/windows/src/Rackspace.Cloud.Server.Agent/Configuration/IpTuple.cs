namespace Rackspace.Cloud.Server.Agent.Configuration {
    public class IpTuple {
        private string _enabled = "1";

        public string ip { set; get; }
        public string netmask { set; get; }
        public string enabled {
            get { return _enabled; }
            set { _enabled = value; }
        }
    }
}