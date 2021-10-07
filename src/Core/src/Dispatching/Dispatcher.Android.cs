#nullable enable
using System;
using Android.OS;

namespace Microsoft.Maui.Dispatching
{
	public class Dispatcher : IDispatcher
	{
		volatile Handler? _handler;

		public bool IsInvokeRequired =>
			Looper.MyLooper() != Looper.MainLooper;

		public void BeginInvokeOnMainThread(Action action)
		{
			if (_handler is null || _handler.Looper != Looper.MainLooper)
				_handler = new Handler(Looper.MainLooper!);

			_handler.Post(action);
		}
	}
}