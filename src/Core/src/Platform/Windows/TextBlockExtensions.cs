using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class TextBlockExtensions
	{
		public static void UpdateFont(this TextBlock nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.FontAttributes.ToFontStyle();
			nativeControl.FontWeight = font.FontAttributes.ToFontWeight();
		}

		public static void UpdateText(this TextBlock nativeControl, IText text)
		{
			nativeControl.Text = text.Text;
		}
	}
}