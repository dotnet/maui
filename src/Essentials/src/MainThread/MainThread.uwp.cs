// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
