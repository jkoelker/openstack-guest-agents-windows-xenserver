using System.Threading;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface ISleeper
    {
        void Sleep(int seconds);
    }

    public class Sleeper : ISleeper
    {
        public void Sleep(int seconds)
        {
            Thread.Sleep(seconds*1000);
        }
    }
}