using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies which edges of a scrolling view's content stay in view while the content changes.
	/// </summary>
	/// <remarks>
	/// The values can be combined: <c>Start | End</c> follows whichever edge the view is at, and the end when the view is at
	/// both because its content fits.
	/// </remarks>
	[Flags]
	public enum ScrollAnchorMode
	{
		/// <summary>No edge is kept in view. This is the default.</summary>
		None = 0,

		/// <summary>
		/// Keeps the start of the content in view while it changes, such as for a feed that adds the newest items at the
		/// start: items added before the first item stay in view instead of above it. Scrolling away from the start stops
		/// following it, and scrolling back to the start follows it again.
		/// </summary>
		Start = 1,

		/// <summary>
		/// Keeps the end of the content in view while it changes: items added or removed, an item growing or shrinking,
		/// a header or footer changing, or the view resizing. Scrolling away from the end stops following it, and
		/// scrolling back to the end follows it again.
		/// </summary>
		End = 2,
	}
}
