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
			_dispatchQueue.Label != DispatchQueue.CurrentQueueLabel;

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
#pragma warning disable BI1234 // Type or member is obsolete
			var q = DispatchQueue.CurrentQueue;
#pragma warning restore BI1234 // Type or member is obsolete
			if (q != DispatchQueue.MainQueue)
				return null;

			return new Dispatcher(q);
		}
	}
}