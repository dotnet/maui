// Based on: https://github.com/xamarin/xamarin-android-tools/blob/d92fc3e3a27e8240551baa813b15d6bf006a5620/src/Microsoft.Android.Build.BaseTasks/AsyncTaskExtensions.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Resizetizer
{
	public static class AsyncTaskExtensions
	{
		/// <summary>
		/// Calls Parallel.ForEach() with appropriate ParallelOptions and exception handling.
		/// </summary>
		public static ParallelLoopResult ParallelForEach<TSource>(this MauiAsyncTask asyncTask, IEnumerable<TSource> source, Action<TSource> body)
		{
			return Parallel.ForEach(source, ParallelOptions(asyncTask),
				s =>
				{
					try
					{
						body(s);
					}
					catch (Exception exc)
					{
						asyncTask.LogCodedError("MAUI0000", exc.ToString());
						asyncTask.Cancel();
					}
				});
		}

		static ParallelOptions ParallelOptions(MauiAsyncTask asyncTask) => new ParallelOptions
		{
			CancellationToken = asyncTask.CancellationToken,
			TaskScheduler = TaskScheduler.Default,
		};

		/// <summary>
		/// Calls Task.Run() with a proper CancellationToken.
		/// </summary>
		public static Task RunTask(this MauiAsyncTask asyncTask, Action body) =>
			Task.Run(body, asyncTask.CancellationToken);


		/// <summary>
		/// Calls Task.Run<T>() with a proper CancellationToken.
		/// </summary>
		public static Task<TSource> RunTask<TSource>(this MauiAsyncTask asyncTask, Func<TSource> body) =>
			Task.Run(body, asyncTask.CancellationToken);
	}
}
