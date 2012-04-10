using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IIPFinder
    {
        List<IPAddress> findIpv6Addresses(string interfaceName);
    }

    public class IPFinder : IIPFinder
    {
        public List<IPAddress> findIpv6Addresses(string interfaceName)
        {
            List<IPAddress> addresses = new List<IPAddress>();

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.Name == interfaceName)
                {
                    IPInterfaceProperties ipInterfaceProperties = networkInterface.GetIPProperties();
                    foreach(IPAddressInformation ipAddressInformation in ipInterfaceProperties.UnicastAddresses)
                    {
                        if (ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            addresses.Add(ipAddressInformation.Address);
                    }
                    break;
                };
            }
            return addresses;
        }
    }
}
