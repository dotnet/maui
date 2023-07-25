using System;
using CoreFoundation;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
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

	/// <inheritdoc/>
	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly DispatchQueue _dispatchQueue;
		DispatchBlock? _dispatchBlock;

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherTimer"/> class.
		/// </summary>
		public DispatcherTimer(DispatchQueue dispatchQueue)
		{
			_dispatchQueue = dispatchQueue;
		}

		/// <inheritdoc/>
		public TimeSpan Interval { get; set; }

		/// <inheritdoc/>
		public bool IsRepeating { get; set; } = true;

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

			_dispatchBlock = new DispatchBlock(OnTimerTick);
			_dispatchQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, Interval), _dispatchBlock);
		}

		/// <inheritdoc/>
		public void Stop()
		{
			if (!IsRunning)
				return;

			IsRunning = false;

			_dispatchBlock?.Cancel();
			_dispatchBlock = null;
		}

		void OnTimerTick()
		{
			if (!IsRunning)
				return;

			Tick?.Invoke(this, EventArgs.Empty);

			if (IsRepeating)
			{
				if (_dispatchBlock is not null)
				{
					_dispatchQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, Interval), _dispatchBlock);
				}
			}
			else
			{
				Stop();
			}
		}
	}

	/// <inheritdoc/>
	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
#pragma warning disable BI1234, CA1416, CA1422 // Type or member is obsolete, has [UnsupportedOSPlatform("ios6.0")], deprecated but still works
			var q = DispatchQueue.CurrentQueue;
#pragma warning restore BI1234, CA1416, CA1422 // Type or member is obsolete
			if (q != DispatchQueue.MainQueue)
				return null;

			return new Dispatcher(q);
		}
	}
}