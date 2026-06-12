namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Provides gesture platform manager instances for a handler connection.
	/// </summary>
	public interface IGesturePlatformManagerProvider
	{
		/// <summary>
		/// Creates a gesture platform manager for the handler connection.
		/// </summary>
		/// <returns>A gesture platform manager owned and disposed by <see cref="GestureManager"/>.</returns>
		IGesturePlatformManager CreateGesturePlatformManager();
	}
}
