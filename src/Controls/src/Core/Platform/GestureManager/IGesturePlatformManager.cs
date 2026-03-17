using System;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11
	/// <summary>
	/// Interface for platform-specific gesture management.
	/// Allows users to provide custom implementations to replace the default gesture handling behavior.
	/// </summary>
	internal interface IGesturePlatformManager : IDisposable
	{
		/// <summary>
		/// Called when a handler is connected to this gesture manager, providing access to the platform view.
		/// Invoked each time a new handler is set, including on reconnection after a disconnect.
		/// </summary>
		void SetupHandler(IViewHandler handler);
	}
}
