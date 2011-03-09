using System;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent.Actions
{
    public interface IInjectFileMessageHandler
    {
        FileInfo Handle(string message);
    }

    public class InjectFileMessageHandler : IInjectFileMessageHandler
    {
        private FileInfo _fileInfo;

        public FileInfo Handle(string message)
        {
            var decodedMessage = "";
            try
            {
                @decodedMessage = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(message));
            }
            catch
            {
                ThrowErrorMessage("");
            }

            if (!IsValid(@decodedMessage)) ThrowErrorMessage(@decodedMessage);

            try
            {

		var splitPoint = @decodedMessage.IndexOf(',');

		var @path = @decodedMessage.Substring(0, splitPoint);
		var content = @decodedMessage.Substring(splitPoint+1);

                if (!@path.IsValidFilePath()) ThrowErrorMessage(@decodedMessage);
                _fileInfo = new FileInfo { Path = @path, Content = content };
            }
            catch
            {
                ThrowErrorMessage(@message);
            }

            return _fileInfo;
        }

        private void ThrowErrorMessage(string @message)
        {
            throw new InvalidCommandException(
                    "File inject message: " + @message + 
                    " is incorrect format. Needs to be something like " +
                    @"C:\somefile.txt,my file content");
        }

        private bool IsValid(string @message)
        {
            return @message.IndexOf(",") > 0;
        }
    }
}
