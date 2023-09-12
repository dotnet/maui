using System;
using Android.OS;
using Android.Systems;
using Java.Lang;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
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

	/// <inheritdoc/>
	partial class DispatcherTimer : IDispatcherTimer
	{
		readonly Handler _handler;
		readonly IRunnable _runnable;

		// For API versions after 29 and later, we can query the Handler directly to ask if callbacks
		// are posted for our IRunnable. For API level before that, we'll need to manually track whether
		// we've posted a callback to the queue.
		bool _hasCallbacks;

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherTimer"/> class.
		/// </summary>
		/// <param name="handler">The handler for this dispatcher to use.</param>
		public DispatcherTimer(Handler handler)
		{
			_runnable = new Runnable(OnTimerTick);
			_handler = handler;
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

			Post();
		}

		/// <inheritdoc/>
		public void Stop()
		{
			if (!IsRunning)
				return;

			IsRunning = false;

			_handler.RemoveCallbacks(_runnable);

			SetHasCallbacks(false);
		}

		void OnTimerTick()
		{
			if (!IsRunning)
				return;

			SetHasCallbacks(false);

			Tick?.Invoke(this, EventArgs.Empty);

			if (IsRepeating)
			{
				Post();
			}
			else
			{
				Stop();
			}
		}

		void Post()
		{
			if (IsCallbackPosted())
			{
				return;
			}

			_handler.PostDelayed(_runnable, (long)Interval.TotalMilliseconds);

			SetHasCallbacks(true);
		}

		bool IsCallbackPosted()
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(29))
			{
				return _handler.HasCallbacks(_runnable);
			}

			// Below API 29 we'll manually track whether there's a posted callback
			return _hasCallbacks;
		}

		void SetHasCallbacks(bool hasCallbacks)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(29))
			{
				return;
			}

			// We only need to worry about tracking this if we're below API 29; after that,
			// we can just ask the Handler with the HasCallBacks() method. 
			_hasCallbacks = hasCallbacks;
		}
	}

	/// <inheritdoc/>
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