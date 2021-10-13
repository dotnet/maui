using System;
using Foundation;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			if (NSThread.Current.IsMainThread)
				return null;

			return new Dispatcher();
		}

		Dispatcher()
		{
		}

		bool IsInvokeRequiredImplementation() =>
			!NSThread.Current.IsMainThread;

		void BeginInvokeOnMainThreadImplementation(Action action) =>
			NSRunLoop.Main.BeginInvokeOnMainThread(() => action());
	}
}