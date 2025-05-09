using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides extension methods for converting between .NET MAUI Graphics font types and SkiaSharp font types.
	/// </summary>
	public static class FontExtensions
	{
		/// <summary>
		/// Converts a .NET MAUI Graphics font to a SkiaSharp typeface.
		/// </summary>
		/// <param name="font">The font to convert.</param>
		/// <returns>A SkiaSharp typeface that corresponds to the specified font.</returns>
		/// <remarks>
		/// If the font name is not found as a family name, this method will attempt to load it as a file.
		/// If the font is null or has an empty name, the default typeface will be returned.
		/// </remarks>
		public static SKTypeface ToSKTypeface(this IFont font)
		{
			if (string.IsNullOrEmpty(font?.Name))
				return SKTypeface.Default;

			try
			{
				return SKTypeface.FromFamilyName(font.Name, font.Weight, (int)SKFontStyleWidth.Normal,
					font.StyleType switch
					{
						FontStyleType.Normal => SKFontStyleSlant.Upright,
						FontStyleType.Italic => SKFontStyleSlant.Italic,
						FontStyleType.Oblique => SKFontStyleSlant.Oblique,
						_ => SKFontStyleSlant.Upright,
					});
			}
			catch
			{
				return SKTypeface.FromFile(font.Name);
			}
		}
	}
}
