#nullable enable
using System;
using Microsoft.UI.Dispatching;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static bool PlatformIsMainThread =>
			TryGetDispatcherQueue()?.HasThreadAccess ?? false;

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			var dispatcher = TryGetDispatcherQueue();

			if (dispatcher == null)
				throw new InvalidOperationException("Unable to find main thread.");

			if (!dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, () => action()))
				throw new InvalidOperationException("Unable to queue on the main thread.");
		}

		static DispatcherQueue? TryGetDispatcherQueue() =>
			DispatcherQueue.GetForCurrentThread() ??
			WindowStateManager.Default.GetActiveWindow(false)?.DispatcherQueue;
	}
}
