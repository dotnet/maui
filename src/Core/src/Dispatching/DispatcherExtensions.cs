using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Dispatching
{
	public static class DispatcherExtensions
	{
		public static void BeginInvokeOnMainThread(this IDispatcher dispatcher, Action action) =>
			dispatcher.BeginInvokeOnMainThread(action);

		public static Task<T> InvokeOnMainThreadAsync<T>(this IDispatcher dispatcher, Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();

			dispatcher.BeginInvokeOnMainThread(() =>
			{
				try
				{
					var result = func();
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});

			return tcs.Task;
		}

		public static Task InvokeOnMainThreadAsync(this IDispatcher dispatcher, Action action) =>
			dispatcher.InvokeOnMainThreadAsync(() =>
			{
				action();
				return true;
			});

		public static Task<T> InvokeOnMainThreadAsync<T>(this IDispatcher dispatcher, Func<Task<T>> funcTask)
		{
			var tcs = new TaskCompletionSource<T>();

			dispatcher.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					var ret = await funcTask().ConfigureAwait(false);
					tcs.SetResult(ret);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			});

			return tcs.Task;
		}

		public static Task InvokeOnMainThreadAsync(this IDispatcher dispatcher, Func<Task> funcTask) =>
			dispatcher.InvokeOnMainThreadAsync(async () =>
			{
				await funcTask().ConfigureAwait(false);
				return true;
			});

		public static Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync(this IDispatcher dispatcher) =>
			dispatcher.InvokeOnMainThreadAsync(() => SynchronizationContext.Current!);
	}
}