#nullable enable
using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// The vsync clock players run on: one per window, plus an application clock for finite runs on targets that
	/// have no window yet. It runs only while something subscribes, so an app that never animates pays nothing.
	/// </summary>
	/// <remarks>
	/// <para>It is separate from the legacy engine's <c>ITicker</c>, whose single <c>Fire</c> delegate can't be
	/// shared, and which a second Android instance stops after two frames at animator scale 0.</para>
	/// <para>Timestamps come from the platform's frame callback, never from a millisecond counter (which
	/// alternates 8 and 9 ms deltas at 120 Hz). <see cref="Elapsed"/> doesn't advance while the clock is stopped
	/// or suspended, so a backgrounded window's animations resume where they were.</para>
	/// </remarks>
	internal abstract partial class AnimationFrameClock : IStoryboardClock
	{
		EventHandler? _tick;
		int _subscribers;
		bool _running;
		bool _suspended;
		bool _destroyed;
		TimeSpan? _origin;
		TimeSpan _elapsedAtStart;

		public TimeSpan Elapsed { get; private set; }

		public bool IsDestroyed => _destroyed;

		/// <summary>Raised when the clock is destroyed with its window; runs on it stop.</summary>
		public event EventHandler? Destroyed;

		public event EventHandler? Tick
		{
			add
			{
				_tick += value;
				_subscribers++;
				UpdateRunning();
			}
			remove
			{
				_tick -= value;
				_subscribers = Math.Max(0, _subscribers - 1);
				UpdateRunning();
			}
		}

		/// <summary>Suspends the clock while its window is stopped. Time doesn't advance until it resumes.</summary>
		public void Suspend()
		{
			_suspended = true;
			UpdateRunning();
		}

		public void Resume()
		{
			_suspended = false;
			UpdateRunning();
		}

		/// <summary>Stops the clock for good and tells every run on it.</summary>
		public void Destroy()
		{
			if (_destroyed)
				return;

			_destroyed = true;
			UpdateRunning();
			_tick = null;
			_subscribers = 0;
			ReleaseFrames();

			var destroyed = Destroyed;
			Destroyed = null;
			if (destroyed is null)
				return;

			// Every run is told, even when one of them throws.
			foreach (EventHandler handler in destroyed.GetInvocationList())
			{
				try
				{
					handler(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<AnimationFrameClock>()?.LogError(ex, "MAUIA1005: An animation run faulted while its window was destroyed.");
				}
			}
		}

		void UpdateRunning()
		{
			var shouldRun = _subscribers > 0 && !_suspended && !_destroyed;
			if (shouldRun == _running)
				return;

			_running = shouldRun;
			if (shouldRun)
			{
				// Map the platform's time line onto the clock's own at the first frame of each run of frames.
				_origin = null;
				_elapsedAtStart = Elapsed;
				StartFrames();
			}
			else
			{
				StopFrames();
			}
		}

		/// <summary>Called by the platform loop with the frame's timestamp, on the window's UI thread.</summary>
		protected void OnFrame(TimeSpan timestamp)
		{
			if (!_running)
				return;

			if (_origin is not TimeSpan origin)
			{
				_origin = origin = timestamp;
			}

			var elapsed = _elapsedAtStart + (timestamp - origin);
			if (elapsed > Elapsed)
				Elapsed = elapsed;

			var tick = _tick;
			if (tick is null)
				return;

			// The frame callback is a fault boundary: players catch their own faults, but nothing may escape into
			// the platform's frame loop, and one subscriber must not stop the others.
			foreach (EventHandler handler in tick.GetInvocationList())
			{
				try
				{
					handler(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<AnimationFrameClock>()?.LogError(ex, "MAUIA1005: An animation run faulted on the frame callback.");
				}
			}
		}

		protected abstract void StartFrames();

		protected abstract void StopFrames();

		/// <summary>Releases the platform frame source for good, when the clock is destroyed with its window.</summary>
		protected virtual void ReleaseFrames()
		{
		}

		static AnimationFrameClock? s_applicationClock;

		/// <summary>Test seam: when set, every run that wasn't given a clock runs on this one.</summary>
		internal static IStoryboardClock? TestClock { get; set; }

		internal static AnimationFrameClock? ApplicationClockIfCreated => s_applicationClock;

		/// <summary>
		/// The application-scoped clock, for finite runs on targets that have no window yet. It lives as long as the
		/// app, so runs on it don't observe its lifetime.
		/// </summary>
		internal static AnimationFrameClock GetApplicationClock()
			=> s_applicationClock ??= Create(Application.Current?.Dispatcher ?? Dispatcher.GetForCurrentThread());

		/// <summary>Creates the platform's vsync clock for a window, or a dispatcher-timer clock where there is none.</summary>
		internal static AnimationFrameClock Create(IDispatcher? dispatcher) => CreatePlatformClock(dispatcher);

		static partial void CreatePlatformClockCore(IDispatcher? dispatcher, ref AnimationFrameClock? clock);

		static AnimationFrameClock CreatePlatformClock(IDispatcher? dispatcher)
		{
			AnimationFrameClock? clock = null;
			CreatePlatformClockCore(dispatcher, ref clock);
			return clock ?? new DispatcherTimerFrameClock(dispatcher);
		}

		/// <summary>
		/// A clock driven by a dispatcher timer, for hosts with no vsync callback (and the netstandard build).
		/// Timestamps come from a monotonic high-resolution counter.
		/// </summary>
		sealed class DispatcherTimerFrameClock : AnimationFrameClock
		{
			static readonly double TicksPerStopwatchTick = TimeSpan.TicksPerSecond / (double)System.Diagnostics.Stopwatch.Frequency;

			readonly IDispatcher? _dispatcher;
			IDispatcherTimer? _timer;

			public DispatcherTimerFrameClock(IDispatcher? dispatcher)
			{
				_dispatcher = dispatcher;
			}

			protected override void StartFrames()
			{
				if (_timer is null)
				{
					var dispatcher = _dispatcher ?? Dispatcher.GetForCurrentThread();
					if (dispatcher is null)
						return;

					try
					{
						_timer = dispatcher.CreateTimer();
					}
					catch (NotImplementedException)
					{
						return;
					}

					_timer.Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0);
					_timer.IsRepeating = true;
					_timer.Tick += OnTimerTick;
				}

				_timer.Start();
			}

			protected override void StopFrames() => _timer?.Stop();

			void OnTimerTick(object? sender, EventArgs e)
				=> OnFrame(TimeSpan.FromTicks((long)(System.Diagnostics.Stopwatch.GetTimestamp() * TicksPerStopwatchTick)));
		}
	}
}
