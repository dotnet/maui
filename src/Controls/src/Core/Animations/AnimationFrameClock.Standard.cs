#nullable enable
using System;
using System.Diagnostics;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Animations
{
	internal abstract partial class AnimationFrameClock
	{
		internal static partial AnimationFrameClock CreatePlatformClock() => new DispatcherTimerClock();

		// No platform vsync source on these targets; a dispatcher timer stands in.
		sealed class DispatcherTimerClock : AnimationFrameClock
		{
			readonly Stopwatch _stopwatch = Stopwatch.StartNew();
			IDispatcherTimer? _timer;

			protected override TimeSpan Now => _stopwatch.Elapsed;

			protected override void StartFrames()
			{
				if (_timer is not null)
					return;

				var dispatcher = Dispatcher.GetForCurrentThread() ?? Application.Current?.Dispatcher
					?? throw new InvalidOperationException("Animations need a dispatcher. In tests, use a ManualStoryboardClock.");

				_timer = dispatcher.CreateTimer();
				_timer.Interval = TimeSpan.FromMilliseconds(16);
				_timer.Tick += OnTimerTick;
				_timer.Start();
			}

			protected override void StopFrames()
			{
				if (_timer is null)
					return;

				_timer.Tick -= OnTimerTick;
				_timer.Stop();
				_timer = null;
			}

			void OnTimerTick(object? sender, EventArgs e) => OnFrame();
		}
	}
}
