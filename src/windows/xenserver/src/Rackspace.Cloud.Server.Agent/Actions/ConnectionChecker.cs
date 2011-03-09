using Rackspace.Cloud.Server.Agent.Configuration;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IConnectionChecker
    {
        void Check();
    }

    public class ConnectionChecker : IConnectionChecker {
        public void Check()
        {
            try
            {
                System.Net.Dns.GetHostEntry("www.google.com");
            }
            catch
            {
                throw new UnsuccessfulCommandExecutionException(
                    "No Network Connection available to update Agent, please try again later!", 
                    new ExecutableResult { ExitCode = "1" }
                    );
            }
        }
    }
}