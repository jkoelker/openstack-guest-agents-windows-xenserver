using System.Timers;
using Rackspace.Cloud.Server.Agent.Interfaces;

namespace Rackspace.Cloud.Server.Agent
{
    public class ProdTimer : ITimer {
        private readonly Timer _timer;
        private ProdTimer(Timer timer) {
            _timer = timer;
        }

        public ProdTimer()
            : this(new Timer()) {
            }

        public bool Enabled {
            get { return _timer.Enabled; }
            set { _timer.Enabled = value; }
        }

        public double Interval {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public void Elapsed(ElapsedEventHandler method) {
            _timer.Elapsed += method;
        }
    }
}