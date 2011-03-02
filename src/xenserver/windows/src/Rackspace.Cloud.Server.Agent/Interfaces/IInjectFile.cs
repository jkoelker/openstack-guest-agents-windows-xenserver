namespace Rackspace.Cloud.Server.Agent.Interfaces
{
    public interface IInjectFile
    {
        void Inject(string path, string content);       
    }
}