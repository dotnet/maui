using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies which edge of a scrolling view's content stays in view while the content changes.
	/// </summary>
	[Flags]
	public enum ScrollAnchorMode
	{
		/// <summary>No edge is kept in view. This is the default.</summary>
		None = 0,

		/// <summary>
		/// Keeps the end of the content in view while it changes: items added or removed, an item growing or shrinking,
		/// a header or footer changing, or the view resizing. Scrolling away from the end stops following it, and
		/// scrolling back to the end follows it again.
		/// </summary>
		End = 2,
	}
}
