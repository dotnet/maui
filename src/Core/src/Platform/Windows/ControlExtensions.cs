using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ControlExtensions
	{
		public static void UpdateFont(this Control nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.FontAttributes.ToFontStyle();
			nativeControl.FontWeight = font.FontAttributes.ToFontWeight();
		}

		public static void UpdateIsEnabled(this Control nativeControl, bool isEnabled)
		{
			nativeControl.IsEnabled = isEnabled;
		}

		public static void UpdateBackgroundColor(this Control nativeControl, Color color)
		{
			nativeControl.Background = color.ToNative();
		}
	}
}