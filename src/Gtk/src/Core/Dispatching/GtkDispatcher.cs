using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Gtk.Dispatching
{
	/// <summary>
	/// <see cref="IDispatcher"/> over the GLib main loop (<c>GLib.Idle</c>/<c>GLib.Timeout</c>).
	/// </summary>
	/// <remarks>
	/// When already on the GTK thread <see cref="Dispatch"/> runs the action inline: GLib's synchronization context
	/// deadlocks when it is asked to <c>Send</c> from its own thread, and MAUI's <c>MainThread</c> bridge only
	/// dispatches when <see cref="IsDispatchRequired"/> is <see langword="true"/> anyway.
	/// </remarks>
	public sealed class GtkDispatcher : IDispatcher
	{
		/// <inheritdoc/>
		public bool IsDispatchRequired => !GtkRuntime.IsGtkThread;

		/// <inheritdoc/>
		public bool Dispatch(Action action)
		{
			if (action is null)
				throw new ArgumentNullException(nameof(action));

			if (!IsDispatchRequired)
			{
				action();
				return true;
			}

			GLib.Idle.Add(() =>
			{
				action();
				return false;
			});
			return true;
		}

		/// <inheritdoc/>
		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			if (action is null)
				throw new ArgumentNullException(nameof(action));

			GLib.Timeout.Add(ToMilliseconds(delay), () =>
			{
				action();
				return false;
			});
			return true;
		}

		/// <inheritdoc/>
		public IDispatcherTimer CreateTimer() => new GtkDispatcherTimer();

		internal static uint ToMilliseconds(TimeSpan interval) =>
			(uint)Math.Max(0, Math.Min(uint.MaxValue, interval.TotalMilliseconds));
	}

	/// <summary>
	/// <see cref="IDispatcherTimer"/> over <c>GLib.Timeout</c>.
	/// </summary>
	public sealed class GtkDispatcherTimer : IDispatcherTimer
	{
		uint _source;
		int _generation;
		TimeSpan _interval = TimeSpan.Zero;

		/// <inheritdoc/>
		public TimeSpan Interval
		{
			get => _interval;
			set
			{
				if (_interval == value)
					return;

				_interval = value;
				if (IsRunning)
				{
					Stop();
					Start();
				}
			}
		}

		/// <inheritdoc/>
		public bool IsRepeating { get; set; }

		/// <inheritdoc/>
		public bool IsRunning => _source != 0;

		/// <inheritdoc/>
		public event EventHandler? Tick;

		/// <inheritdoc/>
		public void Start()
		{
			if (IsRunning)
				return;

			var generation = ++_generation;
			_source = GLib.Timeout.Add(GtkDispatcher.ToMilliseconds(_interval), () => OnTimeout(generation));
		}

		/// <inheritdoc/>
		public void Stop()
		{
			if (_source == 0)
				return;

			var source = _source;
			_source = 0;
			_generation++;
			GLib.Timeout.Remove(source);
		}

		bool OnTimeout(int generation)
		{
			// A stale source (the timer was stopped or restarted since it was added) must die quietly.
			if (generation != _generation)
				return false;

			var repeating = IsRepeating;
			if (!repeating)
				_source = 0;

			Tick?.Invoke(this, EventArgs.Empty);

			// A handler may have stopped (and restarted) the timer: only keep this source alive if it is still the current one.
			return repeating && generation == _generation;
		}
	}

	/// <summary>
	/// <see cref="IDispatcherProvider"/> that hands out the <see cref="GtkDispatcher"/> on the GTK thread and
	/// <see langword="null"/> on every other thread.
	/// </summary>
	public sealed class GtkDispatcherProvider : IDispatcherProvider
	{
		readonly GtkDispatcher _dispatcher = new();

		/// <inheritdoc/>
		public IDispatcher? GetForCurrentThread() => GtkRuntime.IsGtkThread ? _dispatcher : null;
	}
}
