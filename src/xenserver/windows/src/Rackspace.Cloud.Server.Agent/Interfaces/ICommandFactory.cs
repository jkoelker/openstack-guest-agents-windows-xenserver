namespace Rackspace.Cloud.Server.Agent.Interfaces {
    public interface ICommandFactory {
        IExecutableCommand CreateCommand(string name);
    }
}