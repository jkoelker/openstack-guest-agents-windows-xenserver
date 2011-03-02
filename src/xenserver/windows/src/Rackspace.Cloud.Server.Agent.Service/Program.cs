using System.ServiceProcess;

namespace Rackspace.Cloud.Server.Agent.Service {
    static class Program {
        static void Main() {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
                                { 
                                    new Agent.Service.AgentService() 
                                };
            ServiceBase.Run(ServicesToRun);
        }
    }
}