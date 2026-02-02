namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Specifies how the interior of a shape is determined.
	/// </summary>
	public enum FillRule
	{
		/// <summary>
		/// A point is inside the shape if a ray from that point crosses an odd number of path segments.
		/// </summary>
		EvenOdd,
		/// <summary>
		/// A point is inside the shape if the winding count (sum of crossing directions) is nonzero.
		/// </summary>
		Nonzero
	}
}