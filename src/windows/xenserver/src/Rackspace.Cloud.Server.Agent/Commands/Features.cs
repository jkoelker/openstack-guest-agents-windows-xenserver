using System;
using System.Collections.Generic;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent.Commands
{
    public class Features : IExecutableCommand
    {
        public ExecutableResult Execute(string value)
        {
            var enumValues = "";
            foreach (var val in Enum.GetValues(typeof(Utilities.Commands)))
            {
                if (val.ToString() == "features") continue;
                enumValues += val + ",";
            }
            enumValues = enumValues.Substring(0, enumValues.Length - 1);
            return new ExecutableResult { Output = new List<string> { enumValues } };
        }
    }
}