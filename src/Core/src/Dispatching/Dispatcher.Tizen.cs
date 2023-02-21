using System;
using System.Threading;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
	public partial class Dispatcher : IDispatcher
	{
		readonly SynchronizationContext _context;

		internal Dispatcher(SynchronizationContext context)
		{
			_context = context;
		}

		bool IsDispatchRequiredImplementation() =>
			_context != SynchronizationContext.Current;

		bool DispatchImplementation(Action action)
		{
			_context.Post((o) => action(), null);
			return true;
		}

		bool DispatchDelayedImplementation(TimeSpan delay, Action action)
		{
			Timer? timer = null;
			TimerCallback onTimeout = o =>
			{
				_context.Post((o) => action(), null);
				timer?.Dispose();
			};
			timer = new Timer(onTimeout, null, Timeout.Infinite, Timeout.Infinite);
			timer?.Change(delay, delay);
			return true;
		}

		IDispatcherTimer CreateTimerImplementation()
		{
			return new DispatcherTimer(_context);
		}
	}

	/// <inheritdoc/>
	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly SynchronizationContext _context;
		readonly Timer _timer;

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherTimer"/> class.
		/// </summary>
		/// <param name="context">The context for this dispatcher to use.</param>
		public DispatcherTimer(SynchronizationContext context)
		{
			_context = context;
			_timer = new Timer((object? state) => _context.Post(OnTimerTick, null), null, Timeout.Infinite, Timeout.Infinite);
		}

		/// <inheritdoc/>
		public TimeSpan Interval { get; set; }

		/// <inheritdoc/>
		public bool IsRepeating { get; set; }

		/// <inheritdoc/>
		public bool IsRunning { get; private set; }

		/// <inheritdoc/>
		public event EventHandler? Tick;

		/// <inheritdoc/>
		public void Start()
		{
			if (IsRunning)
				return;

			IsRunning = true;
			// set interval separately to prevent calling callback before `timer' is assigned
			_timer.Change(Interval, Interval);
		}

		/// <inheritdoc/>
		public void Stop()
		{
			if (!IsRunning)
				return;

			IsRunning = false;

			_timer.Change(Timeout.Infinite, Timeout.Infinite);
		}


		void OnTimerTick(object? state)
		{
			if (!IsRunning)
				return;

			Tick?.Invoke(this, EventArgs.Empty);

			if (!IsRepeating)
			{
				_timer.Change(Timeout.Infinite, Timeout.Infinite);
			}
		}
	}

	/// <inheritdoc/>
	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var context = SynchronizationContext.Current;
			if (context == null)
				return null;

			return new Dispatcher(context);
		}
	}
}