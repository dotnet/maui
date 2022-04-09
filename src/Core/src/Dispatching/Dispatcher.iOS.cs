using System;
using CoreFoundation;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly DispatchQueue _dispatchQueue;

		internal Dispatcher(DispatchQueue dispatchQueue)
		{
			_dispatchQueue = dispatchQueue;
		}

		bool IsDispatchRequiredImplementation() =>
			_dispatchQueue.Label != DispatchQueue.CurrentQueueLabel;

		bool DispatchImplementation(Action action)
		{
			_dispatchQueue.DispatchAsync(() => action());
			return true;
		}

		bool DispatchDelayedImplementation(TimeSpan delay, Action action)
		{
			_dispatchQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, delay), () => action());
			return true;
		}

		IDispatcherTimer CreateTimerImplementation()
		{
			return new DispatcherTimer(_dispatchQueue);
		}
	}

	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly DispatchQueue _dispatchQueue;
		DispatchBlock? _dispatchBlock;

		public DispatcherTimer(DispatchQueue dispatchQueue)
		{
			_dispatchQueue = dispatchQueue;
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

			_dispatchBlock = new DispatchBlock(OnTimerTick);
			_dispatchQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, Interval), _dispatchBlock);
		}

		public void Stop()
		{
			if (!IsRunning)
				return;

			IsRunning = false;

			_dispatchBlock?.Cancel();
		}

		void OnTimerTick()
		{
			if (!IsRunning)
				return;

			Tick?.Invoke(this, EventArgs.Empty);

			if (IsRepeating)
				_dispatchQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, Interval), _dispatchBlock);
		}
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
#pragma warning disable BI1234 // Type or member is obsolete
			var q = DispatchQueue.CurrentQueue;
#pragma warning restore BI1234 // Type or member is obsolete
			if (q != DispatchQueue.MainQueue)
				return null;

			return new Dispatcher(q);
		}
	}
}