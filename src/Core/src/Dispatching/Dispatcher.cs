using System;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		public static IDispatcher? GetForCurrentThread() =>
			DispatcherProvider.Current.GetForCurrentThread();

		public bool IsInvokeRequired =>
			IsInvokeRequiredImplementation();

		public void BeginInvokeOnMainThread(Action action)
		{
			_ = action ?? throw new ArgumentNullException(nameof(action));

			BeginInvokeOnMainThreadImplementation(action);
		}
	}
}