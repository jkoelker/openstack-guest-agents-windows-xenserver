using System;

namespace Rackspace.Cloud.Server.Agent
{
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class|AttributeTargets.Assembly)]
    public sealed class ExtensionAttribute : Attribute { }
}