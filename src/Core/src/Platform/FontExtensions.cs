namespace Microsoft.Maui
{
	internal static class FontExtensions
	{
		public static Font GetFont(this IFont font) =>
			Font.OfSize(font.FontFamily, font.FontSize).WithAttributes(font.FontAttributes);
	}
}
