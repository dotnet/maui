#nullable enable
using System;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>Clock plumbing shared by visual-state transitions and storyboard players.</summary>
	internal abstract class AnimationRun : IAnimationTrackOwner
	{
		IStoryboardClock? _clock;
		bool _ticking;
		AnimationFrameClock? _observedClock;

		protected IStoryboardClock? Clock => _clock;

		protected bool IsTicking => _ticking;

		protected void StartTicking(IStoryboardClock clock)
		{
			if (_ticking)
				return;

			ObserveClock(clock);
			_clock = clock;
			_ticking = true;
			clock.Tick += OnClockTick;
		}

		protected void StopTicking()
		{
			if (!_ticking || _clock is null)
				return;

			_ticking = false;
			_clock.Tick -= OnClockTick;
		}

		/// <summary>
		/// Observes the destruction of a window's clock independently of ticking, so a paused run, or one that
		/// holds a fill, still ends with its window. The application clock is never observed: it lives forever,
		/// and observing it would keep the run and its target alive.
		/// </summary>
		protected void ObserveClock(IStoryboardClock clock)
		{
			if (ReferenceEquals(_observedClock, clock))
				return;

			StopObservingClock();
			if (clock is AnimationFrameClock { IsWindowClock: true } frameClock)
			{
				_observedClock = frameClock;
				frameClock.Destroyed += OnClockDestroyed;
			}
		}

		protected void StopObservingClock()
		{
			if (_observedClock is null)
				return;

			_observedClock.Destroyed -= OnClockDestroyed;
			_observedClock = null;
		}

		void OnClockTick(object? sender, EventArgs e)
		{
			// A run removed during this frame can still be in the frame's invocation list.
			if (!_ticking)
				return;

			// Fault boundary: a throwing interpolator, coercion or PropertyChanged handler ends this run and
			// releases what it holds, rather than escaping into the platform's frame callback.
			try
			{
				OnTick();
			}
			catch (Exception exception)
			{
				OnFault(exception);
			}
		}

		void OnClockDestroyed(object? sender, EventArgs e) => OnWindowDestroyed();

		protected abstract void OnTick();

		protected abstract void OnWindowDestroyed();

		/// <summary>Tears the run down after <paramref name="exception"/>, releasing everything it holds.</summary>
		protected abstract void OnFault(Exception exception);

		public abstract void OnTrackLost(BindableObject target, BindableProperty property, TrackLossReason reason);

		protected static void LogFault<T>(Exception exception)
			=> MauiLogger<T>.Log(Microsoft.Extensions.Logging.LogLevel.Error, exception, "An animation run faulted and released its values (MAUIA1005).");
	}
}
