namespace Microsoft.Maui.Graphics
{
	public static class IFontExtensions
	{
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
