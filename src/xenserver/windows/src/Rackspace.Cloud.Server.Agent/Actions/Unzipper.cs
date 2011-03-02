using ICSharpCode.SharpZipLib.Zip;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IUnzipper
    {
        void Unzip(string zipFileName, string targetDirectory, string fileFilter);
    }

    public class Unzipper : IUnzipper
    {
        private readonly ILogger _logger;

        public Unzipper(ILogger logger)
        {
            _logger = logger;
        }

        public void Unzip(string zipFileName, string targetDirectory, string fileFilter)
        {
            _logger.Log("Unzipping files");
            new FastZip().ExtractZip(zipFileName, targetDirectory, fileFilter);
            _logger.Log("Unzipping files complete");
        }
    }
}