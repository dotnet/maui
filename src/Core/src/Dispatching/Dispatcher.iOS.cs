using System;
using CoreFoundation;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly DispatchQueue _dispatchQueue;

		internal Dispatcher(DispatchQueue dispatchQueue)
		{
			_dispatchQueue = dispatchQueue;
		}

		bool IsDispatchRequiredImplementation() =>
			_dispatchQueue != DispatchQueue.CurrentQueue;

		bool DispatchImplementation(Action action)
		{
			_dispatchQueue.DispatchAsync(() => action());
			return true;
		}
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var q = DispatchQueue.CurrentQueue;
			if (q != DispatchQueue.MainQueue)
				return null;

			return new Dispatcher(q);
		}
	}
}