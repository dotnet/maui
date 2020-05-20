using System.Timers;

namespace System.Maui.Internals
{
	internal partial class NativeTicker : Ticker
	{
		void Timer_Elapsed(object sender, ElapsedEventArgs e) => SendSignals();

		Timer timer;

		public virtual int MaxFps { get; set; } = 60;

		protected override void DisableTimer()
		{
			if (timer == null)
				return;
			timer.AutoReset = false;
			timer.Stop();
			timer.Elapsed -= Timer_Elapsed;
			timer.Dispose();
			timer = null;
		}

		protected override void EnableTimer()
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
	}
}
