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

		public static void UpdateIsEnabled(this Control nativeControl, bool isEnabled) =>
			nativeControl.IsEnabled = isEnabled;

		public static void UpdateForegroundColor(this Control nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Foreground = color.IsDefault && defaultBrush != null ? defaultBrush : color.ToNative();

		public static void UpdatePadding(this Control nativeControl, Thickness padding, UI.Xaml.Thickness? defaultThickness = null)
		{
			var newPadding = defaultThickness ?? new UI.Xaml.Thickness();

			newPadding.Left += padding.Left;
			newPadding.Top += padding.Top;
			newPadding.Right += padding.Right;
			newPadding.Bottom += padding.Bottom;

			nativeControl.Padding = newPadding;
		}
	}
}