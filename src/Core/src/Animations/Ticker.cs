using System;
using System.Timers;

namespace Microsoft.Maui.Animations
{
	public class Ticker : ITicker
	{
		Timer? _timer;

		public virtual int MaxFps { get; set; } = 60;

		public Action? Fire { get; set; }

		public virtual bool IsRunning => _timer?.Enabled ?? false;

		public virtual bool SystemEnabled => true;

		public virtual void Start()
		{
			if (_timer != null)
				return;

			_timer = new Timer
			{
				AutoReset = true,
				Interval = 1000 / MaxFps,
			};
			_timer.Elapsed += OnTimerElapsed;
			_timer.AutoReset = true;
			_timer.Start();
		}

		public virtual void Stop()
		{
			if (_timer == null)
				return;

			_timer.AutoReset = false;
			_timer.Stop();
			_timer.Elapsed -= OnTimerElapsed;
			_timer.Dispose();
			_timer = null;
		}

		void OnTimerElapsed(object? sender, ElapsedEventArgs e) =>
			Fire?.Invoke();
	}
}