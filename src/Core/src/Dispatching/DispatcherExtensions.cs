using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Dispatching
{
	public static class DispatcherExtensions
	{
		public static Task<T> DispatchAsync<T>(this IDispatcher dispatcher, Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();

			dispatcher.Dispatch(() =>
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

		public static Task DispatchAsync(this IDispatcher dispatcher, Action action) =>
			dispatcher.DispatchAsync(() =>
			{
				action();
				return true;
			});

		public static Task<T> DispatchAsync<T>(this IDispatcher dispatcher, Func<Task<T>> funcTask)
		{
			var tcs = new TaskCompletionSource<T>();

			dispatcher.Dispatch(async () =>
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

		public static Task DispatchAsync(this IDispatcher dispatcher, Func<Task> funcTask) =>
			dispatcher.DispatchAsync(async () =>
			{
				await funcTask().ConfigureAwait(false);
				return true;
			});

		public static Task<SynchronizationContext> GetSynchronizationContextAsync(this IDispatcher dispatcher) =>
			dispatcher.DispatchAsync(() => SynchronizationContext.Current!);

		public static void StartTimer(this IDispatcher dispatcher, TimeSpan interval, Func<bool> callback)
		{
			_ = callback ?? throw new ArgumentNullException(nameof(callback));

			var timer = dispatcher.CreateTimer();
			timer.Interval = interval;
			timer.IsRepeating = true;
			timer.Tick += OnTick;
			timer.Start();

			void OnTick(object? sender, EventArgs e)
			{
				if (!callback())
				{
					timer.Tick -= OnTick;
					timer.Stop();
				}
			}
		}
	}
}