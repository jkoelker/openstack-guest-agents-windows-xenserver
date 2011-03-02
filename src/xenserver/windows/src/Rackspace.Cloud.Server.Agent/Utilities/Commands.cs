using System;

namespace Rackspace.Cloud.Server.Agent.Utilities {
    public enum Commands {
        version,
        password,
        ready,
        resetnetwork,
        [NotUrlEncoded]
        agentupdate,
        xentoolsupdate,
        kmsactivate,
        keyinit,
        injectfile,
        features,
        unrescue,
        updaterupdate
    }

    public class NotUrlEncodedAttribute : Attribute
    {
    }
}
