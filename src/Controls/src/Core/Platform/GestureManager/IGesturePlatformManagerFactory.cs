namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11
	/// <summary>
	/// Creates platform-specific gesture managers for a handler.
	/// </summary>
	/// <remarks>
	/// <see cref="GestureManager"/> owns and disposes the returned manager when the handler disconnects.
	/// Implementations should create a new manager for each call so each handler connection has a distinct
	/// manager lifetime.
	/// </remarks>
	internal interface IGesturePlatformManagerFactory
	{
		/// <summary>
		/// Creates the gesture manager for the specified handler.
		/// </summary>
		IGesturePlatformManager CreateGesturePlatformManager(IViewHandler handler);
	}
}
