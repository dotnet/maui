using System;
using Android.OS;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		readonly Looper _looper;
		readonly Handler _handler;

		internal Dispatcher(Looper looper)
		{
			_looper = looper ?? throw new ArgumentNullException(nameof(looper));
			_handler = new Handler(_looper);
		}

		bool IsDispatchRequiredImplementation() =>
			_looper != Looper.MyLooper();

		bool DispatchImplementation(Action action) =>
			_handler.Post(() => action());
	}

	public partial class DispatcherProvider
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var q = Looper.MyLooper();
			if (q == null || q != Looper.MainLooper)
				return null;

			return new Dispatcher(q);
		}
	}
}