using System;
using Foundation;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		internal Dispatcher()
		{
		}

		bool IsInvokeRequiredImplementation() =>
			!NSThread.Current.IsMainThread;

		void BeginInvokeOnMainThreadImplementation(Action action) =>
			NSRunLoop.Main.BeginInvokeOnMainThread(() => action());
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			if (NSThread.Current.IsMainThread)
				return null;

			return new Dispatcher();
		}
	}
}