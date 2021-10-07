using System;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.DeviceTests
{
	public static class TestMainThread
	{
		public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
		{
#if WINDOWS
			var tcs = new TaskCompletionSource<T>();

			var didQueue = MauiWinUIApplication.Current.MainWindow.DispatcherQueue.TryEnqueue(() =>
			{
				try
				{
					var result = func();
					tcs.TrySetResult(result);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			});

			if (!didQueue)
				throw new Exception("Unable to perform task.");

			return tcs.Task;
#else
			return MainThread.InvokeOnMainThreadAsync(func);
#endif
		}

		public static Task InvokeOnMainThreadAsync(Action action)
		{
#if WINDOWS
			return InvokeOnMainThreadAsync(() => { action(); return true; });
#else
			return MainThread.InvokeOnMainThreadAsync(action);
#endif
		}

		public static Task InvokeOnMainThreadAsync(Func<Task> action)
		{
#if WINDOWS
			return InvokeOnMainThreadAsync(async () => { await action(); return true; });
#else
			return MainThread.InvokeOnMainThreadAsync(action);
#endif
		}

		public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func)
		{
#if WINDOWS
			var tcs = new TaskCompletionSource<T>();

			var didQueue = MauiWinUIApplication.Current.MainWindow.DispatcherQueue.TryEnqueue(async () =>
			{
				try
				{
					var result = await func();
					tcs.TrySetResult(result);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			});

			if (!didQueue)
				throw new Exception("Unable to perform task.");

			return tcs.Task;
#else
			return MainThread.InvokeOnMainThreadAsync(func);
#endif
		}
	}
}
