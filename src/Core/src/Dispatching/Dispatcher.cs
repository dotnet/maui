using System;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		[ThreadStatic]
		static IDispatcher? s_instance;

		public static IDispatcher? GetForCurrentThread() =>
			s_instance ??= DispatcherProvider.Current.CreateDispatcher();

		public bool IsInvokeRequired =>
			IsInvokeRequiredImplementation();

		public void BeginInvokeOnMainThread(Action action)
		{
			_ = action ?? throw new ArgumentNullException(nameof(action));

			BeginInvokeOnMainThreadImplementation(action);
		}
	}
}