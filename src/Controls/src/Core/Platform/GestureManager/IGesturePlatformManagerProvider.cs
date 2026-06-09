namespace Microsoft.Maui.Controls.Platform
{
	// TODO NET11: Make this interface public in .NET 11.
	/// <summary>
	/// Provides gesture platform manager instances for a handler connection.
	/// </summary>
	internal interface IGesturePlatformManagerProvider
	{
		/// <summary>
		/// Creates a gesture platform manager for the handler connection.
		/// </summary>
		/// <returns>A gesture platform manager owned and disposed by <see cref="GestureManager"/>.</returns>
		IGesturePlatformManager CreateGesturePlatformManager();
	}
}
