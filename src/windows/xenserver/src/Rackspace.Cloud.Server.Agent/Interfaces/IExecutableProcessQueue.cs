namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface IExecutableProcessQueue {
        IExecutableProcessQueue Enqueue(string command, string arguments);
        IExecutableProcessQueue Enqueue(string command, string arguments, string[] acceptableExitCodes);
        IExecutableProcessQueue Enqueue(string command, string arguments, bool conditionalToPass);
        void Go();
    }
}
