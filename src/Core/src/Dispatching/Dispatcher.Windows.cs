using System;
using Microsoft.UI.Dispatching;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
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

		DispatcherTimer CreateTimerImplementation()
		{
			return new DispatcherTimer(_dispatcherQueue.CreateTimer());
		}
	}

	/// <inheritdoc/>
	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly DispatcherQueueTimer _timer;

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherTimer"/> class.
		/// </summary>
		/// <param name="timer">An instance of <see cref="DispatcherQueueTimer"/> that will be used for this <see cref="DispatcherTimer"/> instance.</param>
		public DispatcherTimer(DispatcherQueueTimer timer)
		{
			_timer = timer;
		}

		/// <inheritdoc/>
		public TimeSpan Interval
		{
			get => _timer.Interval;
			set => _timer.Interval = value;
		}

		/// <inheritdoc/>
		public bool IsRepeating
		{
			get => _timer.IsRepeating;
			set => _timer.IsRepeating = value;
		}

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

			_timer.Tick += OnTimerTick;

			_timer.Start();
		}

		/// <inheritdoc/>
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

	/// <inheritdoc/>
	public partial class DispatcherProvider
	{
		static Dispatcher? GetForCurrentThreadImplementation()
		{
			var q = DispatcherQueue.GetForCurrentThread();
			if (q == null)
				return null;

			return new Dispatcher(q);
		}
	}
}