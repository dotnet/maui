namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies the types of operations that can be performed on a path.
	/// </summary>
	public enum PathOperation
	{
		/// <summary>
		/// Moves the current point to a new location without drawing a line.
		/// </summary>
		Move,

		/// <summary>
		/// Draws a straight line from the current point to a new point.
		/// </summary>
		Line,

		/// <summary>
		/// Draws a quadratic Bézier curve using the current point, a control point, and an end point.
		/// </summary>
		Quad,

		/// <summary>
		/// Draws a cubic Bézier curve using the current point, two control points, and an end point.
		/// </summary>
		Cubic,

		/// <summary>
		/// Draws an elliptical arc from the current point to a specified point.
		/// </summary>
		Arc,

		/// <summary>
		/// Closes the current subpath by drawing a straight line from the current point to the first point of the subpath.
		/// </summary>
		Close
	}
}
