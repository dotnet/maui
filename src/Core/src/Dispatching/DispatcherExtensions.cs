using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Dispatching
{
	/// <summary>
	/// This class provides a set of extension methods that can be used on objects implementing <see cref="IDispatcher"/>.
	/// </summary>
	public static class DispatcherExtensions
	{
		/// <summary>
		/// Schedules the provided callback on the UI thread from a worker thread, and returns the results asynchronously.
		/// </summary>
		/// <typeparam name="T">The type returned from this method.</typeparam>
		/// <param name="dispatcher">The <see cref="IDispatcher"/> instance this method is called on.</param>
		/// <param name="func">The method to be executed by the dispatcher.</param>
		/// <returns>A <see cref="Task{TResult}"/> object containing information about the state of the dispatcher operation.</returns>
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

		/// <summary>
		/// Schedules the provided action on the UI thread from a worker thread.
		/// </summary>
		/// <param name="dispatcher">The <see cref="IDispatcher"/> instance this method is called on.</param>
		/// <param name="action">The method to be executed by the dispatcher.</param>
		/// <returns><see cref="Task"/>.</returns>
		public static Task DispatchAsync(this IDispatcher dispatcher, Action action) =>
			dispatcher.DispatchAsync(() =>
			{
				action();
				return true;
			});

		/// <summary>
		/// Schedules the provided function on the UI thread from a worker thread.
		/// </summary>
		/// <typeparam name="T">The type returned from this method.</typeparam>
		/// <param name="dispatcher">The <see cref="IDispatcher"/> instance this method is called on.</param>
		/// <param name="funcTask">The function to be executed by the dispatcher.</param>
		/// <returns>A <see cref="Task{TResult}"/> object containing information about the state of the dispatcher operation.</returns>
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

		/// <summary>
		/// Schedules the provided function on the UI thread from a worker thread.
		/// </summary>
		/// <param name="dispatcher">The <see cref="IDispatcher"/> instance this method is called on.</param>
		/// <param name="funcTask">The function to be executed by the dispatcher.</param>
		/// <returns><see langword="Task"/>.</returns>
		public static Task DispatchAsync(this IDispatcher dispatcher, Func<Task> funcTask) =>
			dispatcher.DispatchAsync(async () =>
			{
				await funcTask().ConfigureAwait(false);
				return true;
			});

		/// <summary>
		/// Gets the synchronization context for the current thread.
		/// </summary>
		/// <param name="dispatcher">The <see cref="IDispatcher"/> instance this method is called on.</param>
		/// <returns>A <see cref="SynchronizationContext"/> object representing the current synchronization context.</returns>
		public static Task<SynchronizationContext> GetSynchronizationContextAsync(this IDispatcher dispatcher) =>
			dispatcher.DispatchAsync(() => SynchronizationContext.Current!);

		/// <summary>
		/// Starts a timer on the specified <see cref="IDispatcher"/> context.
		/// </summary>
		/// <param name="dispatcher">The <see cref="IDispatcher"/> instance this method is called on.</param>
		/// <param name="interval">Sets the amount of time between timer ticks.</param>
		/// <param name="callback">The callback on which the dispatcher returns when the event is dispatched.
		/// If the result of the callback is <see langword="true"/>, the timer will repeat, otherwise the timer stops.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
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
