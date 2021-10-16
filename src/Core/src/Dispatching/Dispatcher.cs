using System;

namespace Microsoft.Maui.Dispatching
{
	public partial class Dispatcher : IDispatcher
	{
		// this is mainly settable for unit testing purposes
		static IDispatcherProvider? s_provider;

		[ThreadStatic]
		static IDispatcher? s_instance;

		internal static IDispatcherProvider GetProvider() =>
			s_provider ??= new DispatcherProvider();

		internal static bool SetProvider(IDispatcherProvider? provider)
		{
			if (s_provider == provider)
				return false;

			var old = s_provider;
			s_provider = provider;
			return old != null;
		}

		public static IDispatcher? GetForCurrentThread() =>
			s_instance ??= GetProvider().GetForCurrentThread();

		public bool IsInvokeRequired =>
			IsInvokeRequiredImplementation();

		public void BeginInvokeOnMainThread(Action action) =>
			BeginInvokeOnMainThreadImplementation(action);
	}
}