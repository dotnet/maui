// Based on: https://github.com/xamarin/xamarin-android-tools/blob/d92fc3e3a27e8240551baa813b15d6bf006a5620/src/Microsoft.Android.Build.BaseTasks/AsyncTaskExtensions.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Resizetizer
{
	public static class AsyncTaskExtensions
	{
		/// <summary>
		/// Creates a collection of Task with proper CancellationToken and error handling and waits via Task.WhenAll
		/// </summary>
		public static Task WhenAll<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource> body) =>
			asyncTask.WhenAll(source, body, maxConcurrencyLevel: DefaultMaxConcurrencyLevel);

		/// <summary>
		/// Creates a collection of Task with proper CancellationToken and error handling and waits via Task.WhenAll
		/// </summary>
		public static Task WhenAll<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource> body, int maxConcurrencyLevel, TaskCreationOptions creationOptions = TaskCreationOptions.LongRunning)
		{
			var scheduler = GetTaskScheduler(maxConcurrencyLevel);
			var tasks = new List<Task>();
			foreach (var s in source)
			{
				tasks.Add(Task.Factory.StartNew(() => {
					try
					{
						body(s);
					}
					catch (Exception exc)
					{
						LogErrorAndCancel(asyncTask, exc);
					}
				}, asyncTask.CancellationToken, creationOptions, scheduler));
			}
			return Task.WhenAll(tasks);
		}

		/// <summary>
		/// Creates a collection of Task with proper CancellationToken and error handling and waits via Task.WhenAll
		/// Passes an object the inner method can use for locking. The callback is of the form: (T item, object lockObject)
		/// </summary>
		public static Task WhenAllWithLock<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource, object> body) =>
			asyncTask.WhenAllWithLock(source, body, maxConcurrencyLevel: DefaultMaxConcurrencyLevel);

		/// <summary>
		/// Creates a collection of Task with proper CancellationToken and error handling and waits via Task.WhenAll
		/// Passes an object the inner method can use for locking. The callback is of the form: (T item, object lockObject)
		/// </summary>
		public static Task WhenAllWithLock<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource, object> body, int maxConcurrencyLevel, TaskCreationOptions creationOptions = TaskCreationOptions.LongRunning)
		{
			var scheduler = GetTaskScheduler(maxConcurrencyLevel);
			var lockObject = new object();
			var tasks = new List<Task>();
			foreach (var s in source)
			{
				tasks.Add(Task.Factory.StartNew(() => {
					try
					{
						body(s, lockObject);
					}
					catch (Exception exc)
					{
						LogErrorAndCancel(asyncTask, exc);
					}
				}, asyncTask.CancellationToken, creationOptions, scheduler));
			}
			return Task.WhenAll(tasks);
		}

		/// <summary>
		/// Calls Parallel.ForEach() with appropriate ParallelOptions and exception handling.
		/// </summary>
		public static ParallelLoopResult ParallelForEach<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource> body) =>
			asyncTask.ParallelForEach(source, body, maxConcurrencyLevel: DefaultMaxConcurrencyLevel);

		/// <summary>
		/// Calls Parallel.ForEach() with appropriate ParallelOptions and exception handling.
		/// </summary>
		public static ParallelLoopResult ParallelForEach<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource> body, int maxConcurrencyLevel)
		{
			var options = ParallelOptions(asyncTask, maxConcurrencyLevel);
			return Parallel.ForEach(source, options, s => {
				try
				{
					body(s);
				}
				catch (Exception exc)
				{
					LogErrorAndCancel(asyncTask, exc);
				}
			});
		}

		/// <summary>
		/// Calls Parallel.ForEach() with appropriate ParallelOptions and exception handling.
		/// Passes an object the inner method can use for locking. The callback is of the form: (T item, object lockObject)
		/// </summary>
		public static ParallelLoopResult ParallelForEachWithLock<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource, object> body) =>
			asyncTask.ParallelForEachWithLock(source, body, maxConcurrencyLevel: DefaultMaxConcurrencyLevel);

		/// <summary>
		/// Calls Parallel.ForEach() with appropriate ParallelOptions and exception handling.
		/// Passes an object the inner method can use for locking. The callback is of the form: (T item, object lockObject)
		/// </summary>
		public static ParallelLoopResult ParallelForEachWithLock<TSource>(this AsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource, object> body, int maxConcurrencyLevel)
		{
			var options = ParallelOptions(asyncTask, maxConcurrencyLevel);
			var lockObject = new object();
			return Parallel.ForEach(source, options, s => {
				try
				{
					body(s, lockObject);
				}
				catch (Exception exc)
				{
					LogErrorAndCancel(asyncTask, exc);
				}
			});
		}

		static ParallelOptions ParallelOptions(AsyncTask asyncTask, int maxConcurrencyLevel) => new ParallelOptions
		{
			CancellationToken = asyncTask.CancellationToken,
			TaskScheduler = GetTaskScheduler(maxConcurrencyLevel),
		};

		static TaskScheduler GetTaskScheduler(int maxConcurrencyLevel)
		{
			var pair = new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, maxConcurrencyLevel);
			return pair.ConcurrentScheduler;
		}

		public static int DefaultMaxConcurrencyLevel => Math.Max(1, Environment.ProcessorCount - 1);

		static void LogErrorAndCancel(AsyncTask asyncTask, Exception exc)
		{
			asyncTask.LogCodedError("MAUI0000", exc.ToString());
			asyncTask.Cancel();
		}

		/// <summary>
		/// Calls Task.Factory.StartNew() with a proper CancellationToken, TaskScheduler, and TaskCreationOptions.LongRunning.
		/// </summary>
		public static Task RunTask(this AsyncTask asyncTask, Action body) =>
			asyncTask.RunTask(body, maxConcurrencyLevel: DefaultMaxConcurrencyLevel);

		/// <summary>
		/// Calls Task.Factory.StartNew() with a proper CancellationToken
		/// </summary>
		public static Task RunTask(this AsyncTask asyncTask, Action body, int maxConcurrencyLevel, TaskCreationOptions creationOptions = TaskCreationOptions.LongRunning) =>
			Task.Factory.StartNew(body, asyncTask.CancellationToken, creationOptions, GetTaskScheduler(maxConcurrencyLevel));

		/// <summary>
		/// Calls Task.Factory.StartNew<T>() with a proper CancellationToken, TaskScheduler, and TaskCreationOptions.LongRunning.
		/// </summary>
		public static Task<TSource> RunTask<TSource>(this AsyncTask asyncTask, Func<TSource> body) =>
			asyncTask.RunTask(body, maxConcurrencyLevel: DefaultMaxConcurrencyLevel);

		/// <summary>
		/// Calls Task.Factory.StartNew<T>() with a proper CancellationToken.
		/// </summary>
		public static Task<TSource> RunTask<TSource>(this AsyncTask asyncTask, Func<TSource> body, int maxConcurrencyLevel, TaskCreationOptions creationOptions = TaskCreationOptions.LongRunning) =>
			Task.Factory.StartNew(body, asyncTask.CancellationToken, creationOptions, GetTaskScheduler(maxConcurrencyLevel));
	}
}
