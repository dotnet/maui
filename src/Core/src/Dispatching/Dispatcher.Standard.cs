using System;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		internal Dispatcher()
		{
		}

		bool IsDispatchRequiredImplementation() =>
			throw new NotImplementedException();

		bool DispatchImplementation(Action action) =>
			throw new NotImplementedException();

		bool DispatchDelayedImplementation(TimeSpan delay, Action action) =>
			throw new NotImplementedException();

		IDispatcherTimer CreateTimerImplementation() =>
			throw new NotImplementedException();
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation() => null;
	}
}