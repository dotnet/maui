#nullable enable
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia;

public static class SKFontExtensions
{
	public static SKFont? CreateCopy(this SKFont? font)
	{
		if (font is null)
			return null;

		return new SKFont
		{
			ScaleX = font.ScaleX,
			Size = font.Size,
			SkewX = font.SkewX,
			Typeface = font.Typeface,
		};
	}
}
