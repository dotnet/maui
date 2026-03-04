namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies the style used to join two line segments where they meet.
	/// </summary>
	public enum LineJoin
	{
		/// <summary>
		/// Creates a sharp corner or point. The outer edges of the lines extend until they meet at a point.
		/// If the joint's angle is too sharp, a bevel join is used instead.
		/// </summary>
		Miter,

		/// <summary>
		/// Creates a rounded corner at the joint. The corner is filled with a circle with the diameter equal to the line width.
		/// </summary>
		Round,

		/// <summary>
		/// Creates a beveled corner at the joint. The joint is filled by a triangle that connects the outer edges of the lines.
		/// </summary>
		Bevel
	}
}
