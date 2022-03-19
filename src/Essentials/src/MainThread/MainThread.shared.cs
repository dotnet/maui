using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MainThread']/Docs" />
	public static partial class MainThread
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="//Member[@MemberName='IsMainThread']/Docs" />
		public static bool IsMainThread =>
			PlatformIsMainThread;

		/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="//Member[@MemberName='BeginInvokeOnMainThread']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync'][1]/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync&lt;T&gt;'][2]/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync'][2]/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="//Member[@MemberName='InvokeOnMainThreadAsync&lt;T&gt;'][1]/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/MainThread.xml" path="//Member[@MemberName='GetMainThreadSynchronizationContextAsync']/Docs" />
		public static async Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
		{
			SynchronizationContext ret = null;
			await InvokeOnMainThreadAsync(() =>
				ret = SynchronizationContext.Current).ConfigureAwait(false);
			return ret;
		}
	}
}
