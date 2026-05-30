using System;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11
	/// <summary>
	/// Implemented by a handler that wants to provide its own gesture platform manager
	/// instead of the default one created by <see cref="GestureManager"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="GestureManager"/> owns and disposes the returned instance when the handler disconnects.
	/// A new instance should be returned for each call so each handler connection has a distinct lifetime.
	/// </remarks>
	internal interface ICustomGesturePlatformManagerProvider
	{
		/// <summary>
		/// Creates a platform-specific gesture manager for the implementing handler.
		/// </summary>
		/// <returns>A disposable gesture manager owned by the <see cref="GestureManager"/>.</returns>
		IDisposable CreateGesturePlatformManager();
	}
}
