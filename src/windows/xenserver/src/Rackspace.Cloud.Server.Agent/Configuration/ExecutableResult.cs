using System;
using System.Collections.Generic;
using System.Text;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent.Configuration {
    [Serializable]
    public class ExecutableResult {
        public ExecutableResult() {
            ExitCode = "0";
            Error = new List<string>();
            Output = new List<string>();
        }

        public IEnumerable<string> Output { set; get; }
        public IList<string> Error { set; get; }
        public string ExitCode { set; get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(String.Format("ExitCode={0}", ExitCode));
            stringBuilder.AppendLine(String.Format("Output={0}", Output.Value()));
            stringBuilder.Append(String.Format("Error={0}", Error.Value()));

            return stringBuilder.ToString();
        }
    }
}