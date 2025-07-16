using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Specifies which platform safe area insets to ignore for a layout or visual element.
	/// </summary>
	[Flags]
	public enum SafeAreaRegions
	{
		/// <summary>
		/// Apply platform inset - content will be positioned only in the safe area.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Layout behind the soft input down to where the soft input starts.
		/// </summary>
		SoftInput = 1,

		/// <summary>
		/// Content will never display behind anything that could block it.
		/// If keyboard is visible, content will resize to the visible view.
		/// </summary>
		None = 2,

		/// <summary>
		/// Ignore all insets for this edge - content may be positioned anywhere on the screen, 
		/// including behind toolbars, screen cutouts, etc.
		/// Currently equivalent to ignoring platform safe area insets.
		/// </summary>
		All = int.MaxValue
	}
}