namespace Microsoft.Maui
{
	/// <summary>
	/// Enumeration specifying various options for tab behavior
	/// </summary>
	public enum BarTabMode
	{
		/// <summary>
		/// Auto-sizing tabs behave like Fixed until they exceed the screen space
		/// and then become scrollable.
		/// </summary>
		Auto,

		/// <summary>
		/// All tabs are displayed simultaneously and are compressed to fit the viewport if need be
		/// </summary>
		Fixed,

		/// <summary>
		/// If there are two many tabs to fit on screen, make them scrollable.
		/// </summary>
		Scrollable,
	}
}