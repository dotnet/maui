namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies how text should flow when it exceeds the bounds of its container.
	/// </summary>
	public enum TextFlow
	{
		/// <summary>
		/// Text that exceeds the bounds will be clipped (not visible).
		/// </summary>
		ClipBounds = 0,

		/// <summary>
		/// Text that exceeds the bounds will still be rendered, potentially overlapping with other elements.
		/// </summary>
		OverflowBounds = 1
	}
}
