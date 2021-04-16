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

		public static void UpdateFont(this TextBlock nativeControl, IText text, IFontManager fontManager) =>
			nativeControl.UpdateFont(text.Font, fontManager);

		public static void UpdateText(this TextBlock nativeControl, IText text) =>
			nativeControl.Text = text.Text;

		public static void UpdateForeground(this TextBlock nativeControl, IText text) =>
			nativeControl.UpdateProperty(TextBlock.ForegroundProperty, text.TextColor);

		public static void UpdatePadding(this TextBlock nativeControl, ILabel label) =>
			nativeControl.UpdateProperty(TextBlock.PaddingProperty, label.Padding.ToNative());
	}
}