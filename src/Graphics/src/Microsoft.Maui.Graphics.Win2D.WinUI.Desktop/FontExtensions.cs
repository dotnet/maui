using Microsoft.Graphics.Canvas.Text;
using Windows.UI.Text;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml.Media;
#endif

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
			=> new CanvasTextFormat
			{
				FontFamily = font?.Name ?? FontFamily.XamlAutoFontFamily.Source,
				FontSize = size,
				FontWeight = new FontWeight { Weight = (ushort)(font?.Weight ?? FontWeights.Regular) },
				FontStyle = (font?.StyleType ?? FontStyleType.Normal).ToFontStyle()
			};
	}
}
