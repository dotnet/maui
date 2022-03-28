using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Microsoft.Maui.ApplicationModel
{
	static class PlatformUtils
	{
		internal const string AppManifestFilename = "AppxManifest.xml";
		internal const string AppManifestXmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
		internal const string AppManifestUapXmlns = "http://schemas.microsoft.com/appx/manifest/uap/windows10";

		internal static void WatchForError(this IAsyncAction self) =>
			self.AsTask().WatchForError();

		internal static void WatchForError<T>(this IAsyncOperation<T> self) =>
			self.AsTask().WatchForError();

		internal static void WatchForError(this Task self)
		{
			var context = SynchronizationContext.Current;
			if (context == null)
				return;

			self.ContinueWith(
				t =>
				{
					var exception = t.Exception.InnerExceptions.Count > 1 ? t.Exception : t.Exception.InnerException;

					context.Post(e => { throw (Exception)e; }, exception);
				}, CancellationToken.None,
				TaskContinuationOptions.OnlyOnFaulted,
				TaskScheduler.Default);
		}
	}
}
