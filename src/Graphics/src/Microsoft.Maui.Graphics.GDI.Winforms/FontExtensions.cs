using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDFont = System.Drawing.Font;

namespace Microsoft.Maui.Graphics
{
	internal static class FontExtensions
	{
		public static SDFont ToSystemDrawingFont(this IFont font, float emSize)
			=> new SDFont(font?.Name ?? SystemFonts.DefaultFont.FontFamily.Name, emSize, (font?.StyleType ?? FontStyleType.Normal) switch
			{
				FontStyleType.Normal => FontStyle.Regular,
				FontStyleType.Italic => FontStyle.Italic,
				FontStyleType.Oblique => FontStyle.Italic,
				_ => FontStyle.Regular,
			});
	}
}
