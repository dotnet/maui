#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.UnitTests
{
	class DispatcherStub : IDispatcher
	{
		readonly Func<bool>? _isInvokeRequired;
		readonly Action<Action>? _invokeOnMainThread;

		public DispatcherStub(Func<bool>? isInvokeRequired, Action<Action>? invokeOnMainThread)
		{
			_isInvokeRequired = isInvokeRequired;
			_invokeOnMainThread = invokeOnMainThread;

			ManagedThreadId = Environment.CurrentManagedThreadId;
		}

		public bool IsDispatchRequired =>
			_isInvokeRequired?.Invoke() ?? false;

		public int ManagedThreadId { get; }

		public bool Dispatch(Action action)
		{
			if (_invokeOnMainThread is null)
				action();
			else
				_invokeOnMainThread.Invoke(action);
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			Timer? timer = null;

			timer = new Timer(OnTimeout, null, delay, delay);

			return true;

			void OnTimeout(object? state)
			{
				Dispatch(action);

				timer?.Dispose();
			}
		}

		public IDispatcherTimer CreateTimer()
		{
			return new DispatcherTimerStub(this);
		}
	}

	class DispatcherTimerStub : IDispatcherTimer
	{
		readonly DispatcherStub _dispatcher;

		Timer? _timer;

		public DispatcherTimerStub(DispatcherStub dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public TimeSpan Interval { get; set; }

		public bool IsRepeating { get; set; }

		public bool IsRunning => _timer != null;

		public event EventHandler? Tick;

		public void Start()
		{
			_timer = new Timer(OnTimeout, null, Interval, IsRepeating ? Interval : Timeout.InfiniteTimeSpan);

			void OnTimeout(object? state)
			{
				_dispatcher.Dispatch(() => Tick?.Invoke(this, EventArgs.Empty));
			}
		}

		public void Stop()
		{
			_timer?.Dispose();
			_timer = null;
		}
	}

	class DispatcherProviderStub : IDispatcherProvider, IDisposable
	{
		ThreadLocal<IDispatcher?> s_dispatcherInstance = new(() =>
			DispatcherProviderStubOptions.SkipDispatcherCreation
				? null
				: new DispatcherStub(
					DispatcherProviderStubOptions.IsInvokeRequired,
					DispatcherProviderStubOptions.InvokeOnMainThread));

		public void Dispose() =>
			s_dispatcherInstance.Dispose();

		public IDispatcher? GetForCurrentThread()
		{
			var x = s_dispatcherInstance.Value;

			if (x == null)
			{
				System.Diagnostics.Debug.WriteLine("WTH");
			}

			return x;
		}
	}

	public class DispatcherProviderStubOptions
	{
		[ThreadStatic]
		public static bool SkipDispatcherCreation;

		[ThreadStatic]
		public static Func<bool>? IsInvokeRequired;

		[ThreadStatic]
		public static Action<Action>? InvokeOnMainThread;
	}

	public static class DispatcherTest
	{
		public static Task Run(Action testAction) =>
			Run(() =>
			{
				testAction();
				return Task.CompletedTask;
			});

		public static Task Run(Func<Task> testAction)
		{
			var tcs = new TaskCompletionSource();

			var thread = new Thread(async () =>
			{
				try
				{
					await testAction();

					tcs.SetResult();
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});
			thread.Start();

			return tcs.Task;
		}
	}
}