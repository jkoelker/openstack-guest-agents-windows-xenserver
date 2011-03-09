using System.Text.RegularExpressions;
using Rackspace.Cloud.Server.Common.AgentUpdate;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IAgentUpdateMessageHandler
    {
        AgentUpdateInfo Handle(string message);
    }

    public class AgentUpdateMessageHandler : IAgentUpdateMessageHandler {

        public AgentUpdateInfo Handle(string message)
        {
            if (!IsValid(message))
            {
                throw new InvalidCommandException(
                    string.Format(
                        "Update message: {0}, is incorrect format.  Need 'http://tempuri/file.zip,md5valueOfzipfile'",
                        message));    
            }
            var info = message.Split(new[] { ',' });
            return new AgentUpdateInfo { url = info[0], signature = info[1] };
        }

        private bool IsValid(string message)
        {
            const string pattern = @"^http:\/\/(.*),[a-zA-Z0-9]+$";
            return Regex.IsMatch(message, pattern);
        }
    }
}