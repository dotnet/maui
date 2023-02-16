using System;
using System.Timers;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class Ticker : ITicker
	{
		Timer? _timer;

		/// <inheritdoc/>
		public virtual int MaxFps { get; set; } = 60;

		/// <inheritdoc/>
		public Action? Fire { get; set; }

		/// <inheritdoc/>
		public virtual bool IsRunning => _timer?.Enabled ?? false;

		/// <inheritdoc/>
		public virtual bool SystemEnabled => true;

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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