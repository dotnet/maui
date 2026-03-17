using System;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11
	/// <summary>
	/// Interface for platform-specific gesture management.
	/// Allows users to provide custom implementations to replace the default gesture handling behavior.
	/// </summary>
	/// <remarks>
	/// Implementations that store per-view state (via <see cref="SetupHandler"/>) should be registered
	/// as <b>transient</b> or <b>scoped</b> in the dependency injection container, not as singletons.
	/// A singleton registration will cause <see cref="SetupHandler"/> to be called with different handlers
	/// from different views, and only the last-connected view will receive correct gesture handling.
	/// </remarks>
	internal interface IGesturePlatformManager : IDisposable
	{
		/// <summary>
		/// Called when a handler is connected to this gesture manager, providing access to the platform view.
		/// Invoked each time a new handler is set, including on reconnection after a disconnect.
		/// </summary>
		void SetupHandler(IViewHandler handler);
	}
}
