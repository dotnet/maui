using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Graphics.Skia
{
	public static class FontExtensions
	{
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
