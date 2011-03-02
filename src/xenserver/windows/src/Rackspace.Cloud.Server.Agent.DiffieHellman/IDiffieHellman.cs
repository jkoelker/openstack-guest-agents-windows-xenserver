namespace Rackspace.Cloud.Server.DiffieHellman {
    public interface IDiffieHellman {
        string CreateKeyExchange();
        string DecryptKeyExchange(string key);
    }
}