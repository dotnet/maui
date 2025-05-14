#nullable enable
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia;

/// <summary>
/// Provides extension methods for SkiaSharp font objects.
/// </summary>
public static class SKFontExtensions
{
	/// <summary>
	/// Creates a deep copy of a SkiaSharp font object.
	/// </summary>
	/// <param name="font">The font object to copy.</param>
	/// <returns>A new font object with the same properties as the original, or null if the input is null.</returns>
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
