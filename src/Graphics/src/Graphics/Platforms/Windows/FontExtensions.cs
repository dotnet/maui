using Microsoft.Graphics.Canvas.Text;
using Windows.UI.Text;
using System;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml.Media;
#endif

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
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

		public static CanvasTextFormat ToCanvasTextFormat(this IFont font, float size, string text = "")
		{
			var direction = IsRightToLeft(text)
				? CanvasTextDirection.RightToLeftThenTopToBottom
				: CanvasTextDirection.LeftToRightThenTopToBottom;

			return new CanvasTextFormat
			{
				FontFamily = font?.Name ?? FontFamily.XamlAutoFontFamily.Source,
				FontSize = size,
				// Ensure font weight stays within the valid range (1–999) to avoid runtime errors
				FontWeight = new FontWeight { Weight = (ushort)Math.Clamp(font?.Weight ?? FontWeights.Regular, 1, 999) },
				FontStyle = (font?.StyleType ?? FontStyleType.Normal).ToFontStyle()
			};
		}

		static bool IsRightToLeft(string text)
		{
			// Check for known RTL Unicode ranges (e.g., Arabic, Hebrew, Urdu)
			foreach (char c in text)
			{
				// RTL Unicode block ranges: 0590–08FF
				if ((c >= '\u0590' && c <= '\u08FF') || (c >= '\uFB1D' && c <= '\uFDFF') || (c >= '\uFE70' && c <= '\uFEFF'))
					return true;
				break;
			}
			return false;
		}

	}
}
