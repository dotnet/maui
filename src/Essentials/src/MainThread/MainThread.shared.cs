using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// The MainThread class allows applications to run code on the main thread of execution, and to determine if a particular block of code is currently running on the main thread.
	/// </summary>
	public static partial class MainThread
	{
		/// <summary>
		/// True if the current thread is the UI thread.
		/// </summary>
		public static bool IsMainThread =>
			PlatformIsMainThread;

		/// <summary>
		/// Invokes an action on the main thread of the application.
		/// </summary>
		/// <param name="action">The action to invoke on the main thread.</param>
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

		/// <summary>
		/// Invokes an action on the main thread of the application asynchronously.
		/// </summary>
		/// <param name="action">The action to invoke on the main thread.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
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

		/// <summary>
		/// Invokes a function on the main thread of the application asynchronously.
		/// </summary>
		/// <typeparam name="T">Type of the object to be returned.</typeparam>
		/// <param name="func">The function task to execute on the main thread.</param>
		/// <returns>A <see cref="Task"/> object that can be awaited to capture the result object.</returns>
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

		/// <summary>
		/// Invokes a function on the main thread of the application asynchronously.
		/// </summary>
		/// <param name="funcTask">The function task to execute on the main thread.</param>
		/// <returns>A <see cref="Task"/> object that can be awaited.</returns>
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

		/// <summary>
		/// Invokes a function on the main thread of the application asynchronously.
		/// </summary>
		/// <typeparam name="T">Type of the object to be returned.</typeparam>
		/// <param name="funcTask">The function task to execute on the main thread.</param>
		/// <returns>A <see cref="Task"/> object that can be awaited to capture the result object.</returns>
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

		/// <summary>
		/// Gets the main thread synchonization context.
		/// </summary>
		/// <returns>The synchronization context for the main thread.</returns>
		public static async Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
		{
			SynchronizationContext ret = null;
			await InvokeOnMainThreadAsync(() =>
				ret = SynchronizationContext.Current).ConfigureAwait(false);
			return ret;
		}
	}
}
