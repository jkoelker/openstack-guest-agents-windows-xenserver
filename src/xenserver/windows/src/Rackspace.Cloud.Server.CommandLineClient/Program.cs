using System;
using System.Net.NetworkInformation;
using Rackspace.Cloud.Server.Agent;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;
using Rackspace.Cloud.Server.Common.Logging;
using StructureMap;
using NetworkInterface=System.Net.NetworkInformation.NetworkInterface;

namespace Rackspace.Cloud.Server.CommandLineClient {
    public class CmdXenStore : XenStore {
        public CmdXenStore(IExecutableProcess executableProcess)
            : base(executableProcess) {
        }
    }

    public class Program {
        static void Main(string[] args) {

            //StructureMapConfiguration.UseDefaultStructureMapConfigFile = false;
            //StructureMapConfiguration.BuildInstancesOf<IXenStore>().TheDefaultIsConcreteType<CmdXenStore>();

            //new IoC().Register();

            //ObjectFactory.GetInstance<ICommandQueue>().Work();

            //Console.WriteLine("DONE !!!");

            var process = new ExecutableProcess(new Logger(), new ExecutableProcessCommandPatternSubsitution());
            var result = process.Run("netsh", "interface set interface name=\"public\" newname=\"p(*)lic\"");

            Console.WriteLine("Result Code: " + result.ExitCode);
            Console.WriteLine("Output: " + result.Output.Value());
            Console.WriteLine("Error: " + result.Error.Value());
        }

        private static void ShowNetworkInterfaces() {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            Console.WriteLine("Interface information for {0}.{1}     ",
                    computerProperties.HostName, computerProperties.DomainName);
            if (nics == null || nics.Length < 1) {
                Console.WriteLine("  No network interfaces found.");
                return;
            }

            Console.WriteLine("  Number of interfaces .................... : {0}", nics.Length);
            foreach (NetworkInterface adapter in nics) {
                IPInterfaceProperties properties = adapter.GetIPProperties(); //  .GetIPInterfaceProperties();
                Console.WriteLine();
                Console.WriteLine(adapter.Description);
                Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                Console.Write("  Physical address ........................ : ");
                PhysicalAddress address = adapter.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();
                for (int i = 0; i < bytes.Length; i++) {
                    // Display the physical address in hexadecimal.
                    Console.Write("{0}", bytes[i].ToString("X2"));
                    // Insert a hyphen after each byte, unless we are at the end of the 
                    // address.
                    if (i != bytes.Length - 1) {
                        Console.Write("-");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
