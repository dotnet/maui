using System;

namespace Microsoft.Maui.Dispatching
{
	/// <inheritdoc/>
	public partial class DispatcherProvider : IDispatcherProvider
	{
		[ThreadStatic]
		static IDispatcher? s_dispatcherInstance;

		// this is mainly settable for unit testing purposes
		static IDispatcherProvider? s_currentProvider;

		/// <summary>
		/// Gets the currently set <see cref="IDispatcherProvider"/> instance.
		/// </summary>
		public static IDispatcherProvider Current =>
			s_currentProvider ??= new DispatcherProvider();

		/// <summary>
		/// Sets the current dispatcher provider.
		/// </summary>
		/// <param name="provider">The <see cref="IDispatcherProvider"/> object to set as the current dispatcher provider.</param>
		/// <returns><see langword="true"/> if the current dispatcher was actually updated, otherwise <see langword="false"/>.</returns>
		public static bool SetCurrent(IDispatcherProvider? provider)
		{
			if (s_currentProvider == provider)
				return false;

			var old = s_currentProvider;
			s_currentProvider = provider;
			return old != null;
		}

		/// <inheritdoc/>
		public IDispatcher? GetForCurrentThread() =>
			s_dispatcherInstance ??= GetForCurrentThreadImplementation();
	}
}