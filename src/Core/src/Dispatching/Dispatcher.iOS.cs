#nullable enable
using System;
using Foundation;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		public bool IsInvokeRequired =>
			!NSThread.Current.IsMainThread;

		public void BeginInvokeOnMainThread(Action action) =>
			NSRunLoop.Main.BeginInvokeOnMainThread(action);
	}
}