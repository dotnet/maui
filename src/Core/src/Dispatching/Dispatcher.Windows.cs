using System;
using Microsoft.UI.Dispatching;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly DispatcherQueue _dispatcherQueue;

		internal Dispatcher(DispatcherQueue dispatcherQueue)
		{
			_dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
		}

		bool IsDispatchRequiredImplementation() =>
			!_dispatcherQueue.HasThreadAccess;

		bool DispatchImplementation(Action action) =>
			_dispatcherQueue.TryEnqueue(() => action());

		bool DispatchDelayedImplementation(TimeSpan delay, Action action)
		{
			var timer = _dispatcherQueue.CreateTimer();
			timer.Interval = delay;
			timer.Tick += OnTimerTick;
			timer.Start();
			return true;

			void OnTimerTick(DispatcherQueueTimer sender, object args)
			{
				action();
				timer.Tick -= OnTimerTick;
			}
		}

		IDispatcherTimer CreateTimerImplementation()
		{
			return new DispatcherTimer(_dispatcherQueue.CreateTimer());
		}
	}

	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly DispatcherQueueTimer _timer;

		public DispatcherTimer(DispatcherQueueTimer timer)
		{
			_timer = timer;
		}

		public TimeSpan Interval
		{
			get => _timer.Interval;
			set => _timer.Interval = value;
		}

		public bool IsRepeating
		{
			get => _timer.IsRepeating;
			set => _timer.IsRepeating = value;
		}

		public bool IsRunning { get; private set; }

		public event EventHandler? Tick;

		public void Start()
		{
			if (IsRunning)
				return;

			IsRunning = true;

			_timer.Tick += OnTimerTick;

			_timer.Start();
		}

		public void Stop()
		{
			if (!IsRunning)
				return;

			IsRunning = false;

			_timer.Tick -= OnTimerTick;

			_timer.Stop();
		}

		void OnTimerTick(DispatcherQueueTimer sender, object args)
		{
			Tick?.Invoke(this, EventArgs.Empty);

			if (!IsRepeating)
				Stop();
		}
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var q = DispatcherQueue.GetForCurrentThread();
			if (q == null)
				return null;

			return new Dispatcher(q);
		}
	}
}