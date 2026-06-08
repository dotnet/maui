namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11.
	/// <summary>
	/// Creates gesture platform manager instances for a handler connection.
	/// </summary>
	internal interface IGesturePlatformManagerFactory
	{
		/// <summary>
		/// Creates a gesture platform manager for the specified handler.
		/// </summary>
		/// <param name="handler">The handler connected to the view that owns the gesture manager.</param>
		/// <returns>A gesture platform manager owned and disposed by <see cref="GestureManager"/>.</returns>
		IGesturePlatformManager CreateGesturePlatformManager(IViewHandler handler);
	}
}
