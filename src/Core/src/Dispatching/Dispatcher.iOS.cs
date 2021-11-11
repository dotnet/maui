using System;
using Foundation;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly NSRunLoop _runLoop;

		internal Dispatcher(NSRunLoop runLoop)
		{
			_runLoop = runLoop;
		}

		bool IsInvokeRequiredImplementation() =>
			_runLoop != NSRunLoop.Main;

		void BeginInvokeOnMainThreadImplementation(Action action) =>
			_runLoop.BeginInvokeOnMainThread(() => action());
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var rl = NSRunLoop.Current;
			if (rl != NSRunLoop.Main)
				return null;

			return new Dispatcher(rl);
		}
	}
}