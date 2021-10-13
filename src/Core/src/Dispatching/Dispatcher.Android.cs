using System;
using Android.OS;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		static IDispatcher? GetForCurrentThreadImplementation()
		{
			var q = Looper.MyLooper();
			if (q == null || q != Looper.MainLooper)
				return null;

			return new Dispatcher(q);
		}

		readonly Looper _looper;
		readonly Handler _handler;

		Dispatcher(Looper looper)
		{
			_looper = looper ?? throw new ArgumentNullException(nameof(looper));
			_handler = new Handler(_looper);
		}

		bool IsInvokeRequiredImplementation() =>
			_looper != Looper.MainLooper;

		void BeginInvokeOnMainThreadImplementation(Action action) =>
			_handler.Post(() => action());
	}
}