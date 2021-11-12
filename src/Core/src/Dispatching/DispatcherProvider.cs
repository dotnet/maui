using System;
using System.ComponentModel;

namespace Microsoft.Maui.Dispatching
{
	public partial class DispatcherProvider : IDispatcherProvider
	{
		[ThreadStatic]
		static IDispatcher? s_dispatcherInstance;

		// this is mainly settable for unit testing purposes
		static IDispatcherProvider? s_currentProvider;

		public static IDispatcherProvider Current =>
			s_currentProvider ??= new DispatcherProvider();

		public static bool SetCurrent(IDispatcherProvider? provider)
		{
			if (s_currentProvider == provider)
				return false;

			var old = s_currentProvider;
			s_currentProvider = provider;
			return old != null;
		}

		public IDispatcher? GetForCurrentThread() =>
			s_dispatcherInstance ??= GetForCurrentThreadImplementation();
	}
}