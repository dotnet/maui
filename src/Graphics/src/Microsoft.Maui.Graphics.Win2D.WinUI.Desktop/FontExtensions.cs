using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace Microsoft.Maui.Graphics.Win2D
{
	internal static class FontExtensions
	{
		public static FontStyle ToFontStyle(this FontStyleType fontStyleType)
			=> fontStyleType switch
			{
				FontStyleType.Italic => FontStyle.Italic,
				FontStyleType.Oblique => FontStyle.Oblique,
				FontStyleType.Normal => FontStyle.Normal,
				_ => FontStyle.Normal
			};

		public static CanvasTextFormat ToCanvasTextFormat(this IFont font, float size)
			=>new CanvasTextFormat
			{
				FontFamily = font?.Name ?? FontFamily.XamlAutoFontFamily.Source,
				FontSize = size,
				FontWeight = new FontWeight((ushort)(font?.Weight ?? FontWeights.Regular)),
				FontStyle = (font?.StyleType ?? FontStyleType.Normal).ToFontStyle()
			};
	}
}
