using System;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Lets a backend built over the platform-less (<c>net10.0</c>) assemblies attach its own gesture handling. A backend assigns
	/// <see cref="Factory"/> (for example the GTK backend); with the default <see langword="null"/> nothing is attached and the
	/// behaviour is the same as before.
	/// </summary>
	internal static class GesturePlatformBackend
	{
		/// <summary>
		/// Creates the gesture connection of a view handler, or returns <see langword="null"/> when the handler's platform view is not one
		/// the backend handles. The connection is disposed when the handler or its container changes.
		/// </summary>
		public static Func<IViewHandler, IDisposable?>? Factory { get; set; }
	}

	class GesturePlatformManager : IDisposable
	{
		IDisposable? _connection;

		public GesturePlatformManager(IViewHandler handler)
		{
			_connection = GesturePlatformBackend.Factory?.Invoke(handler);
		}

		public void Dispose()
		{
			_connection?.Dispose();
			_connection = null;
		}
	}
}
