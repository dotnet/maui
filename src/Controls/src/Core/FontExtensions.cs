using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public static class FontExtensions
	{
		public static Font WithAttributes(this Font font, FontAttributes attributes)
		{
			var bold = (attributes & FontAttributes.Bold) != 0;
			var italic = (attributes & FontAttributes.Italic) != 0;
			return font.WithWeight(bold ? FontWeight.Bold : FontWeight.Regular, italic ? FontSlant.Italic : FontSlant.Default);
		}

		public static FontAttributes GetFontAttributes(this Font font)
		{
			FontAttributes attributes = font.Weight == FontWeight.Bold ? FontAttributes.Bold : FontAttributes.None;
			if (font.Slant != FontSlant.Default)
			{
				if (attributes == FontAttributes.None)
					attributes = FontAttributes.Italic;
				else
					attributes = attributes | FontAttributes.Italic;
			}
			return attributes;
		}

		public static Font ToFont(this IFontElement element, double? defaultSize = null)
		{
			var size = element.FontSize;
			if (defaultSize.HasValue && (size <= 0 || double.IsNaN(size)))
				size = defaultSize.Value;

			return Font.OfSize(element.FontFamily, size, enableScaling: element.FontAutoScalingEnabled).WithAttributes(element.FontAttributes);
		}
	}
}
