namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies how items align to snap points in a scrollable items layout.
	/// </summary>
	/// <remarks>
	/// Snap points cause the scrolling to "snap" to specific positions, creating a paging-like effect.
	/// This enum controls where items align when they snap into position.
	/// </remarks>
	public enum SnapPointsAlignment
	{
		/// <summary>
		/// Items snap to align with the start of the scrollable area.
		/// For vertical scrolling, items align to the top. For horizontal scrolling, items align to the left (or right in RTL).
		/// </summary>
		Start,
		
		/// <summary>
		/// Items snap to align with the center of the scrollable area.
		/// The snapped item appears centered in the visible viewport.
		/// </summary>
		Center,
		
		/// <summary>
		/// Items snap to align with the end of the scrollable area.
		/// For vertical scrolling, items align to the bottom. For horizontal scrolling, items align to the right (or left in RTL).
		/// </summary>
		End
	}
}