#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies which platform safe area insets to ignore for a layout or visual element.
	/// </summary>
	[Flags]
	public enum SafeAreaGroup
	{
		/// <summary>
		/// Apply platform inset - content will be positioned only in the safe area.
		/// </summary>
		None = 0,

		/// <summary>
		/// Ignore all insets for this edge - content may be positioned anywhere on the screen, 
		/// including behind toolbars, screen cutouts, etc.
		/// Currently equivalent to ignoring platform safe area insets.
		/// </summary>
		All = 1 << 0

		// Note: When adding new flags in the future, update the 'All' value to include all applicable flags:
		// All = (1 << 0) | (new flags...)
	}
}