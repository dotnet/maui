using System;
using System.Timers;

namespace Microsoft.Maui.Animations
{
	public class Ticker : ITicker
	{ 
        void Timer_Elapsed(object sender, ElapsedEventArgs e) => Fire?.Invoke();

        Timer? timer;

        public virtual int MaxFps { get; set; } = 60;
		public Action? Fire { get; set; }
        public virtual bool IsRunning => timer?.Enabled ?? false;
        public virtual bool SystemEnabled => true;

        public virtual void Start()
        {
            if (timer != null)
            {
                return;
            }
            timer = new Timer
            {
                AutoReset = true,
                Interval = 1000 / MaxFps,
            };
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }
        public virtual void Stop()
        {
            if (timer == null)
                return;
            timer.AutoReset = false;
            timer.Stop();
            timer.Elapsed -= Timer_Elapsed;
            timer.Dispose();
            timer = null;
        }
    }
}
