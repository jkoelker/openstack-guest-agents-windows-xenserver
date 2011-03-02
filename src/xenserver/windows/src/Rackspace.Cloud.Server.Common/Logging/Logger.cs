namespace Rackspace.Cloud.Server.Common.Logging
{
    public class Logger : ILogger {
        public void Log(string content) {
            if(!LogManager.ShouldBeLogging) return;
            LogManager.Instance.Info(content);
        }
    }
}