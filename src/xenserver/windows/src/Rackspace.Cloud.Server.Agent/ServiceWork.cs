using Rackspace.Cloud.Server.Agent.Interfaces;
using Rackspace.Cloud.Server.Agent.Utilities;

namespace Rackspace.Cloud.Server.Agent {
    public class ServiceWork {
        private readonly ITimer _timer;
        private readonly ICommandQueue _queue;

        public ServiceWork(ITimer timer, ICommandQueue queue) {
            _timer = timer;
            _queue = queue;
        }

        public void Do() {
            _timer.Enabled = false;
            _queue.Work();
            if(Statics.ShouldPollXenStore)  _timer.Enabled = true;
        }
    }
}