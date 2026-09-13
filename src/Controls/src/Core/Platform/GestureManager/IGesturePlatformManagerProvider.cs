namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Provides a per-handler <see cref="IGesturePlatformManager"/> for a specific handler connection.
	/// This is an internal escape hatch for handler-scoped customization; application-wide customization
	/// should use the public <see cref="IGesturePlatformManagerFactory"/> registered via DI.
	/// The factory takes precedence when both are present.
	/// </summary>
	internal interface IGesturePlatformManagerProvider
	{
		/// <summary>
		/// Creates a new gesture platform manager for the handler connection.
		/// </summary>
		/// <returns>
		/// A new gesture platform manager instance owned and disposed by <see cref="GestureManager"/>.
		/// This method must return a new instance for each call because <see cref="GestureManager"/>
		/// disposes and recreates the manager on each connect or handler change.
		/// </returns>
		IGesturePlatformManager CreateGesturePlatformManager();
	}
}
