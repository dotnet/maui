namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Provides gesture platform manager instances for a handler connection.
	/// </summary>
	public interface IGesturePlatformManagerProvider
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
