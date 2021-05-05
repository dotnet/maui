#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	public static class FontRegistrarExtensions
	{
		public static bool TryGetFont(this IFontRegistrar fontRegistrar, string font, out string? fontPath)
		{
			var (hasFont, foundFontPath) = fontRegistrar.HasFont(font);
			if (hasFont)
				fontPath = foundFontPath!;
			else
				fontPath = null;
			return hasFont;
		}
	}
}