#nullable enable
using System;
using Microsoft.UI.Dispatching;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly DispatcherQueue _dispatcherQueue;

		public Dispatcher()
			: this(DispatcherQueue.GetForCurrentThread())
		{
		}

		public Dispatcher(DispatcherQueue dispatcherQueue)
		{
			_dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
		}

		public bool IsInvokeRequired =>
			!_dispatcherQueue.HasThreadAccess;

		public void BeginInvokeOnMainThread(Action action) =>
			_dispatcherQueue.TryEnqueue(() => action());
	}
}