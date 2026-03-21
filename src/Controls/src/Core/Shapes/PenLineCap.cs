namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>Specifies the shape at the end of a line or segment.</summary>
	public enum PenLineCap
	{
		/// <summary>A cap that does not extend beyond the last point of the line.</summary>
		Flat,
		/// <summary>A rectangle with height equal to the line thickness and length equal to half the line thickness.</summary>
		Square,
		/// <summary>A semicircle with a diameter equal to the line thickness.</summary>
		Round
	}
}
