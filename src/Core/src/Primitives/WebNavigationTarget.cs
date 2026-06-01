namespace Microsoft.Maui
{
	/// <summary>
	/// Specifies the frame target of a web navigation.
	/// </summary>
	public enum WebNavigationTarget
	{
		/// <summary>
		/// The navigation targets the main (top-level) frame.
		/// </summary>
		MainFrame,
		/// <summary>
		/// The navigation targets a child frame (e.g., an iframe).
		/// </summary>
		Frame,
		/// <summary>
		/// The navigation requests a new window (e.g., target="_blank" or window.open).
		/// </summary>
		NewWindow
	}
}
