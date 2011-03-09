using System.Timers;

namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface ITimer {
        bool Enabled { get; set; }
        void Elapsed(ElapsedEventHandler method);
    }
}