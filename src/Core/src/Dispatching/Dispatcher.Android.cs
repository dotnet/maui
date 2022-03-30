using System;
using Android.OS;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly Looper _looper;
		readonly Handler _handler;

		internal Dispatcher(Looper looper)
		{
			_looper = looper ?? throw new ArgumentNullException(nameof(looper));
			_handler = new Handler(_looper);
		}

		bool IsDispatchRequiredImplementation() =>
			_looper != Looper.MyLooper();

		bool DispatchImplementation(Action action) =>
			_handler.Post(() => action());

		bool DispatchDelayedImplementation(TimeSpan delay, Action action) =>
			_handler.PostDelayed(() => action(), (long)delay.TotalMilliseconds);

		IDispatcherTimer CreateTimerImplementation()
		{
			return new DispatcherTimer(_handler);
		}
	}

	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly Handler _handler;

		public DispatcherTimer(Handler handler)
		{
			_handler = handler;
		}

		public TimeSpan Interval { get; set; }

		public bool IsRepeating { get; set; } = true;

		public bool IsRunning { get; private set; }

		public event EventHandler? Tick;

		public void Start()
		{
			if (IsRunning)
				return;

			IsRunning = true;

			_handler.PostDelayed(OnTimerTick, (long)Interval.TotalMilliseconds);
		}

		public void Stop()
		{
			if (!IsRunning)
				return;

			IsRunning = false;

			_handler.RemoveCallbacks(OnTimerTick);
		}

		void OnTimerTick()
		{
			if (!IsRunning)
				return;

			Tick?.Invoke(this, EventArgs.Empty);

			if (IsRepeating)
				_handler.PostDelayed(OnTimerTick, (long)Interval.TotalMilliseconds);
		}
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var q = Looper.MyLooper();
			if (q == null || q != Looper.MainLooper)
				return null;

			return new Dispatcher(q);
		}
	}
}