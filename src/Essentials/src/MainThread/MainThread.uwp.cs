using System;
using System.Diagnostics;

#if WINDOWS_UWP
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
#elif WINDOWS
using Microsoft.System;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread
		{
			get
			{
#if WINDOWS_UWP
				// if there is no main window, then this is either a service
				// or the UI is not yet constructed, so the main thread is the
				// current thread
				try
				{
					if (CoreApplication.MainView?.CoreWindow == null)
						return true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Unable to validate MainView creation. {ex.Message}");
					return true;
				}

				return CoreApplication.MainView.CoreWindow.Dispatcher?.HasThreadAccess ?? false;
#elif WINDOWS
				return DispatcherQueue.GetForCurrentThread()?.HasThreadAccess ?? false;
#endif
			}
		}

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
#if WINDOWS_UWP
			var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;

			if (dispatcher == null)
				throw new InvalidOperationException("Unable to find main thread.");
			dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
#elif WINDOWS
			var dispatcher = DispatcherQueue.GetForCurrentThread();

			if (dispatcher == null)
				throw new InvalidOperationException("Unable to find main thread.");

			if (!dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, () => action()))
				throw new InvalidOperationException("Unable to queue on the main thread.");
#endif
		}
	}
}
