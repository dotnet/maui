namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Resolved from the handler's <see cref="Microsoft.Maui.IMauiContext.Services"/> to create the
	/// <see cref="IGesturePlatformManager"/> used for a handler connection. Register a custom
	/// implementation to override gesture handling behavior application-wide.
	/// </summary>
	public interface IGesturePlatformManagerFactory
	{
		/// <summary>
		/// Creates a new gesture platform manager for the supplied handler connection.
		/// </summary>
		/// <param name="handler">The handler whose platform view the gestures are attached to.</param>
		/// <returns>
		/// A new gesture platform manager instance owned and disposed by <see cref="GestureManager"/>.
		/// This method must return a new instance for each call because <see cref="GestureManager"/>
		/// disposes and recreates the manager on each connect or handler change.
		/// </returns>
		IGesturePlatformManager CreateGesturePlatformManager(IViewHandler handler);
	}
}
