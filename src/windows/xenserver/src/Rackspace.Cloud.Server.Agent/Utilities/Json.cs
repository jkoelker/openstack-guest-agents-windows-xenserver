using System;
using System.Web.Script.Serialization;
using Rackspace.Cloud.Server.Agent.Configuration;

namespace Rackspace.Cloud.Server.Agent.Utilities
{
    public class Json<T>
    {
        public T Deserialize(string json)
        {
            try
            {
                return new JavaScriptSerializer(new SimpleTypeResolver()).Deserialize<T>(json);
            }
            catch
            {
                throw new UnsuccessfulCommandExecutionException(
                    String.Format("Problem deserializing the following json: '{0}'", json),
                    new ExecutableResult { ExitCode = "1" });
            }
        }

        public string Serialize(T objectToSerialize)
        {
            try
            {
                return new JavaScriptSerializer().Serialize(objectToSerialize);
            }
            catch
            {
                throw new UnsuccessfulCommandExecutionException(
                    String.Format("Problem serializing the following object: '{0}'", objectToSerialize),
                    new ExecutableResult { ExitCode = "1" });
            }
        }
    }
}
