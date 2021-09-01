using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal static class TaskExtensions
	{
		public static void WatchForError(this IAsyncAction self)
		{
			self.AsTask().WatchForError();
		}

		public static void WatchForError<T>(this IAsyncOperation<T> self)
		{
			self.AsTask().WatchForError();
		}

		public static void WatchForError(this Task self)
		{
			SynchronizationContext context = SynchronizationContext.Current;
			if (context == null)
				return;

			self.ContinueWith(t =>
			{
				Exception exception = t.Exception.InnerExceptions.Count > 1 ? t.Exception : t.Exception.InnerException;

				context.Post(e => { throw (Exception)e; }, exception);
			}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
		}
	}
}