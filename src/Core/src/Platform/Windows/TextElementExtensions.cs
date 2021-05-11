using Microsoft.UI.Xaml.Documents;

namespace Microsoft.Maui
{
	public static class TextElementExtensions
	{
		public static void UpdateFont(this TextElement nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.ToFontStyle();
			nativeControl.FontWeight = font.ToFontWeight();
		}
	}
}