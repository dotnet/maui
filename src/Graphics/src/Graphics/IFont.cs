namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a font with name, weight, and style properties for text rendering.
	/// </summary>
	public interface IFont
	{
		/// <summary>
		/// Gets the name of the font family or font file.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the weight of the font (boldness level).
		/// </summary>
		/// <remarks>
		/// Standard values are defined in the <see cref="FontWeights"/> constants.
		/// </remarks>
		int Weight { get; }

		/// <summary>
		/// Gets the style type of the font (e.g., normal, italic).
		/// </summary>
		FontStyleType StyleType { get; }
	}
}
