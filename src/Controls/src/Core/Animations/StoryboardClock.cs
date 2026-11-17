#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// A source of animation time. Players sample their timelines when the clock ticks.
	/// </summary>
	public interface IStoryboardClock
	{
		/// <summary>
		/// Gets the current time of the clock.
		/// </summary>
		TimeSpan Elapsed { get; }

		/// <summary>
		/// Raised once per frame, after <see cref="Elapsed"/> has advanced.
		/// </summary>
		event EventHandler? Tick;
	}

	/// <summary>
	/// A clock that advances only when <see cref="Advance(TimeSpan)"/> is called, for deterministic tests.
	/// </summary>
	public sealed class ManualStoryboardClock : IStoryboardClock
	{
		/// <summary>
		/// Gets the current time of the clock.
		/// </summary>
		public TimeSpan Elapsed { get; private set; }

		/// <summary>
		/// Raised synchronously by <see cref="Advance(TimeSpan)"/>.
		/// </summary>
		public event EventHandler? Tick;

		/// <summary>
		/// Advances the clock by <paramref name="delta"/> and raises <see cref="Tick"/> synchronously.
		/// </summary>
		/// <param name="delta">The amount of time to advance. Must not be negative.</param>
		public void Advance(TimeSpan delta)
		{
			if (delta < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(delta));

			Elapsed += delta;
			Tick?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	/// A vsync-driven clock. It delivers frames only while something listens, uses the platform's frame
	/// timestamps, and doesn't advance while suspended (the window is stopped).
	/// </summary>
	internal abstract partial class AnimationFrameClock : IStoryboardClock
	{
		EventHandler? _tick;
		int _subscribers;
		bool _suspended;
		TimeSpan _suspendedAt;
		TimeSpan _suspendedTotal;

		/// <summary>Monotonic platform time: the latest frame's timestamp while frames run, the current time otherwise.</summary>
		protected abstract TimeSpan Now { get; }

		protected abstract void StartFrames();

		protected abstract void StopFrames();

		public TimeSpan Elapsed => (_suspended ? _suspendedAt : Now) - _suspendedTotal;

		public event EventHandler? Tick
		{
			add
			{
				_tick += value;
				if (++_subscribers == 1 && !_suspended)
					StartFrames();
			}
			remove
			{
				_tick -= value;
				if (--_subscribers == 0)
					StopFrames();
			}
		}

		/// <summary>Raised when the window that owns this clock is destroyed. Runs on it stop.</summary>
		public event EventHandler? Destroyed;

		/// <summary>Whether a window owns this clock. The application clock is never destroyed.</summary>
		public bool IsWindowClock { get; set; }

		protected void OnFrame()
		{
			if (!_suspended)
				_tick?.Invoke(this, EventArgs.Empty);
		}

		public void Suspend()
		{
			if (_suspended)
				return;

			_suspendedAt = Now;
			_suspended = true;
			if (_subscribers > 0)
				StopFrames();
		}

		public void Resume()
		{
			if (!_suspended)
				return;

			if (_subscribers > 0)
				StartFrames();

			_suspendedTotal += Now - _suspendedAt;
			_suspended = false;
		}

		public void Destroy()
		{
			Destroyed?.Invoke(this, EventArgs.Empty);
			if (_subscribers > 0)
				StopFrames();
		}

		internal static partial AnimationFrameClock CreatePlatformClock();
	}

	/// <summary>Chooses the clock a run uses: one per window, or an application clock for elements without a window.</summary>
	internal static class AnimationClocks
	{
		static readonly ConditionalWeakTable<Window, AnimationFrameClock> s_windowClocks = new();
		static AnimationFrameClock? s_applicationClock;

		/// <summary>When set, every run uses this clock. For unit tests.</summary>
		internal static IStoryboardClock? ClockOverride { get; set; }

		public static IStoryboardClock For(BindableObject target)
		{
			if (ClockOverride is { } clock)
				return clock;

			if (target is VisualElement { Window: { } window })
				return s_windowClocks.GetValue(window, CreateWindowClock);

			return s_applicationClock ??= AnimationFrameClock.CreatePlatformClock();
		}

		static AnimationFrameClock CreateWindowClock(Window window)
		{
			var clock = AnimationFrameClock.CreatePlatformClock();
			clock.IsWindowClock = true;
			_ = new WindowClockLifetime(window, clock);
			return clock;
		}

		// Follows the window's lifecycle. A destroyed window can come back: Android keeps a MainPage window when the
		// Activity is recreated, and the new Activity raises Created, not Resumed. So a destroyed clock is dropped,
		// and the window gets a fresh one on its next run.
		sealed class WindowClockLifetime
		{
			readonly Window _window;
			readonly AnimationFrameClock _clock;

			public WindowClockLifetime(Window window, AnimationFrameClock clock)
			{
				_window = window;
				_clock = clock;
				window.Stopped += OnStopped;
				window.Resumed += OnResumed;
				window.Activated += OnResumed;
				window.Destroying += OnDestroying;
			}

			void OnStopped(object? sender, EventArgs e) => _clock.Suspend();

			void OnResumed(object? sender, EventArgs e) => _clock.Resume();

			void OnDestroying(object? sender, EventArgs e)
			{
				_window.Stopped -= OnStopped;
				_window.Resumed -= OnResumed;
				_window.Activated -= OnResumed;
				_window.Destroying -= OnDestroying;
				s_windowClocks.Remove(_window);
				_clock.Destroy();
			}
		}
	}
}
