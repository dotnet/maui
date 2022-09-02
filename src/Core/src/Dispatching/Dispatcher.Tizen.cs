using System;
using System.Threading;

namespace Microsoft.Maui.Dispatching
{
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

	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly SynchronizationContext _context;
		readonly Timer _timer;

		public DispatcherTimer(SynchronizationContext context)
		{
			_context = context;
			_timer = new Timer((object? state) => _context.Post(OnTimerTick, null), null, Timeout.Infinite, Timeout.Infinite);
		}

		public TimeSpan Interval { get; set; }

		public bool IsRepeating { get; set; }

		public bool IsRunning { get; private set; }

		public event EventHandler? Tick;

		public void Start()
		{
			if (IsRunning)
				return;

			IsRunning = true;
			// set interval separarately to prevent calling callback before `timer' is assigned
			_timer.Change(Interval, Interval);
		}

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