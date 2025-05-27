namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies the style of line cap (ending) to use at the end of lines when drawing.
	/// </summary>
	public enum LineCap
	{
		/// <summary>
		/// The line ends at the endpoint with no extension.
		/// </summary>
		Butt,

		/// <summary>
		/// The line is capped with a semicircle whose diameter equals the line thickness.
		/// </summary>
		Round,

		/// <summary>
		/// The line is capped with a square that has the same width as the line thickness and extends beyond the end point by half the line thickness.
		/// </summary>
		Square
	}
}
