using System.ComponentModel;

namespace Microsoft.Maui.Dispatching
{
	public partial class DispatcherProvider : IDispatcherProvider
	{
		// this is mainly settable for unit testing purposes
		static IDispatcherProvider? s_current;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDispatcherProvider Current =>
			s_current ??= new DispatcherProvider();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool SetCurrent(IDispatcherProvider? provider)
		{
			if (s_current == provider)
				return false;

			var old = s_current;
			s_current = provider;
			return old != null;
		}

		public IDispatcher? CreateDispatcher() =>
			GetForCurrentThreadImplementation();
	}
}