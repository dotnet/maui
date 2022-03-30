using System;
using Microsoft.UI.Dispatching;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread =>
			WindowStateManager.Default.GetActiveWindow().DispatcherQueue.HasThreadAccess;

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			var dispatcher = WindowStateManager.Default.GetActiveWindow().DispatcherQueue;

			if (dispatcher == null)
				throw new InvalidOperationException("Unable to find main thread.");

			if (!dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, () => action()))
				throw new InvalidOperationException("Unable to queue on the main thread.");
		}
	}
}
