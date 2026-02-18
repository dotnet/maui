namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Specifies the shape at the vertices where two lines meet.
	/// </summary>
	public enum PenLineJoin
	{
		/// <summary>
		/// A pointed corner where two lines meet.
		/// </summary>
		Miter,

		/// <summary>
		/// A beveled (cut-off) corner where two lines meet.
		/// </summary>
		Bevel,

		/// <summary>
		/// A rounded corner where two lines meet.
		/// </summary>
		Round
	}
}