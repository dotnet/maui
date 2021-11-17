using System;
using System.Threading;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly SynchronizationContext _context;

		internal Dispatcher(SynchronizationContext context)
		{
			_context = context;
		}

		bool IsInvokeRequiredImplementation() =>
			_context != SynchronizationContext.Current;

		void BeginInvokeOnMainThreadImplementation(Action action) =>
			_context.Post((o) => action(), null);
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var context = SynchronizationContext.Current;
			if (context == null)
				return null;

			return new Dispatcher(context);
		}
	}
}