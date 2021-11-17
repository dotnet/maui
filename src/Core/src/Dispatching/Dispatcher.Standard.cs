using System;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		internal Dispatcher()
		{
		}

		bool IsInvokeRequiredImplementation() =>
			throw new NotImplementedException();

		void BeginInvokeOnMainThreadImplementation(Action action) =>
			throw new NotImplementedException();
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation() => null;
	}
}