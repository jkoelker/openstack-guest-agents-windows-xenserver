using System.IO;
using System.Net;
using Rackspace.Cloud.Server.Common.Configuration;
using Rackspace.Cloud.Server.Common.Logging;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IDownloader
    {
        void Download(string url, string pathToDownloadTo);
    }

    public class Downloader : IDownloader
    {
        private readonly ILogger _logger;

        public Downloader(ILogger logger)
        {
            _logger = logger;
        }

        public void Download(string url, string pathToDownloadTo)
        {
            _logger.Log("Downloading ...");

            if (!Directory.Exists(SvcConfiguration.AgentVersionUpdatesPath))
                Directory.CreateDirectory(SvcConfiguration.AgentVersionUpdatesPath);

            var webClient = new WebClient();
            webClient.DownloadFile(url, pathToDownloadTo);

            _logger.Log("Downloaded.");
        }
    }
}