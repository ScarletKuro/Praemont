using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NoteCore.Model
{
    public interface ITimer : IDisposable
    {
        void Start();
        double Interval { set; }
        event EventHandler Elapsed;
    }

    public sealed class SysTimer : ITimer
    {
        private Timer _timer = new Timer();

        private SysTimer()
        {
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public double Interval
        {
            set { _timer.Interval = value; }
        }

        public event EventHandler Elapsed;

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                Elapsed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                // Timers eat exceptions. We don't
                ThreadPool.QueueUserWorkItem(_ => { throw e; });
            }
        }

        public static Func<ITimer> ImplementationOverride { private get; set; }

        public static ITimer Factory()
        {
            return (ImplementationOverride != null) ? ImplementationOverride() : new SysTimer();
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_timer == null) return;
            _timer.Stop();
            _timer.Elapsed -= TimerOnElapsed;
            _timer.Dispose();
            _timer = null;
        }

        public sealed class Timers : IDisposable
        {
            private bool _disposed;
            private List<ITimer> _timers = new List<ITimer>();

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _timers.ForEach(timer => timer.Dispose());
                _timers.Clear();
                _timers = null;
            }

            public void Add(int seconds, EventHandler handler)
            {
                if (_disposed) throw new ObjectDisposedException(string.Empty);
                var timer = SysTimer.Factory();
                timer.Interval = 100;
                timer.Elapsed += (s, e) =>
                {
                    timer.Interval = seconds * 1000;
                    handler(s, e);
                };
                timer.Start();
            }
        }
    }
}