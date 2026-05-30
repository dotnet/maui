using System;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this delegate public in .NET 11
	/// <summary>
	/// Creates a platform-specific gesture manager for the specified handler.
	/// Allows users to provide a custom implementation to replace the default gesture handling behavior.
	/// </summary>
	/// <remarks>
	/// <see cref="GestureManager"/> owns and disposes the returned instance when the handler disconnects.
	/// A new instance should be created for each call so each handler connection has a distinct lifetime.
	/// </remarks>
	internal delegate IDisposable GesturePlatformManagerCreator(IViewHandler handler);
}
