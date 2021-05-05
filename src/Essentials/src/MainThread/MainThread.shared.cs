using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class MainThread
	{
		public static bool IsMainThread =>
			PlatformIsMainThread;

		public static void BeginInvokeOnMainThread(Action action)
		{
			if (IsMainThread)
			{
				action();
			}
			else
			{
				PlatformBeginInvokeOnMainThread(action);
			}
		}

		public static Task InvokeOnMainThreadAsync(Action action)
		{
			if (IsMainThread)
			{
				action();
				return Task.CompletedTask;
			}

			var tcs = new TaskCompletionSource<bool>();

			BeginInvokeOnMainThread(() =>
			{
				try
				{
					action();
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			});

			return tcs.Task;
		}

		public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
		{
			if (IsMainThread)
			{
				return Task.FromResult(func());
			}

			var tcs = new TaskCompletionSource<T>();

			BeginInvokeOnMainThread(() =>
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

			return tcs.Task;
		}

		public static Task InvokeOnMainThreadAsync(Func<Task> funcTask)
		{
			if (IsMainThread)
			{
				return funcTask();
			}

			var tcs = new TaskCompletionSource<object>();

			BeginInvokeOnMainThread(
				async () =>
				{
					try
					{
						await funcTask().ConfigureAwait(false);
						tcs.SetResult(null);
					}
					catch (Exception e)
					{
						tcs.SetException(e);
					}
				});

			return tcs.Task;
		}

		public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
		{
			if (IsMainThread)
			{
				return funcTask();
			}

			var tcs = new TaskCompletionSource<T>();

			BeginInvokeOnMainThread(
				async () =>
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

		public static async Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
		{
			SynchronizationContext ret = null;
			await InvokeOnMainThreadAsync(() =>
				ret = SynchronizationContext.Current).ConfigureAwait(false);
			return ret;
		}
	}
}
