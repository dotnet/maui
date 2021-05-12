#nullable enable

namespace Microsoft.Maui
{
	public static class FontRegistrarExtensions
	{
		public static (bool hasFont, string? fontPath) HasFont(this IFontRegistrar fontRegistrar, string font)
		{
			var foundFontPath = fontRegistrar.GetFont(font);
			var hasFont = !string.IsNullOrEmpty(foundFontPath);

			return (hasFont, foundFontPath);
		}
	}
}