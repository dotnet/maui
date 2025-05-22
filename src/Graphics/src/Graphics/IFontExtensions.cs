namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IFont"/> interface.
	/// </summary>
	public static class IFontExtensions
	{
		/// <summary>
		/// Gets the SVG-compatible weight value for the font.
		/// </summary>
		/// <param name="font">The font to get the SVG weight for.</param>
		/// <returns>A string representing the font weight in SVG format, or null if the font is null.</returns>
		public static string GetSvgWeight(this IFont font)
		{
			if (font == null)
				return null;

			if (font.Weight == FontWeights.Normal)
				return "normal";

			if (font.Weight == FontWeights.Regular)
				return "normal";

			if (font.Weight == FontWeights.Bold)
				return "bold";

			return font.Weight.ToInvariantString();
		}

		/// <summary>
		/// Gets the SVG-compatible style value for the font.
		/// </summary>
		/// <param name="font">The font to get the SVG style for.</param>
		/// <returns>A string representing the font style in SVG format, or null if the font is null.</returns>
		public static string GetSvgStyle(this IFont font)
		{
			if (font == null)
				return null;

			if (font.StyleType == FontStyleType.Italic)
				return "italic";

			if (font.StyleType == FontStyleType.Oblique)
				return "oblique";

			return "normal";
		}
	}
}
