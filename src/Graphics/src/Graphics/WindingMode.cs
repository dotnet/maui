namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies the algorithm used to determine which regions are inside or outside a path for filling.
	/// </summary>
	public enum WindingMode
	{
		/// <summary>
		/// Uses the non-zero winding rule, which fills any region with a non-zero winding number.
		/// A point is inside the shape if a ray from that point to infinity crosses path segments with a non-zero net number of clockwise versus counter-clockwise crossings.
		/// </summary>
		NonZero = 0,

		/// <summary>
		/// Uses the even-odd rule, which fills regions based on alternating crossings.
		/// A point is inside the shape if a ray from that point to infinity crosses an odd number of path segments.
		/// </summary>
		EvenOdd = 1
	}
}
