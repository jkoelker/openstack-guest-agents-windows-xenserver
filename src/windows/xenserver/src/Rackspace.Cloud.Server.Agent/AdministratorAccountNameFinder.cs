using System;
using System.Management;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent {

    public class AdministratorAccountNameFinder : IAdministratorAccountNameFinder {
        public string Find()
        {
            var msc = new ManagementScope("\\root\\cimv2");
            const string QUERY_STRING = "SELECT * FROM Win32_UserAccount";
            var q = new SelectQuery(QUERY_STRING);
            var query = new ManagementObjectSearcher(msc,q);
            var queryCollection = query.Get();

            var administratorAccountName = "";
            foreach( ManagementObject mo in queryCollection ) 
            {
                var sid = mo["SID"].ToString();
                if (sid.LastIndexOf("-500") != (sid.Length - 4)) continue;
                
                administratorAccountName = String.Format("{0}", mo["Name"]); 
            }

            return administratorAccountName;
        }
    }

}