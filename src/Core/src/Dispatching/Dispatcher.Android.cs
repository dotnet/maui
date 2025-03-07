using System;
using Android.OS;
using Android.Systems;
using Java.Lang;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
	public partial class Dispatcher : IDispatcher
	{
		// NOTE: PlatformDispatcher extends Handler
		readonly PlatformDispatcher _dispatcher;

		internal Dispatcher(PlatformDispatcher dispatcher)
		{
			_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
		}

		bool IsDispatchRequiredImplementation() =>
			_dispatcher.IsDispatchRequired;

		bool DispatchImplementation(Action action) =>
			_dispatcher.Post(() => action());

		bool DispatchDelayedImplementation(TimeSpan delay, Action action) =>
			_dispatcher.PostDelayed(() => action(), (long)delay.TotalMilliseconds);

		DispatcherTimer CreateTimerImplementation()
		{
			return new DispatcherTimer(_dispatcher);
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
		static Dispatcher? GetForCurrentThreadImplementation()
		{
			var dispatcher = PlatformDispatcher.Create();
			if (dispatcher is null)
				return null;

			return new Dispatcher(dispatcher);
		}
	}
}