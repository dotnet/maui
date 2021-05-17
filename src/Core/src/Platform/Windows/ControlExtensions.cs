#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ControlExtensions
	{
		public static void UpdateFont(this Control nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.ToFontStyle();
			nativeControl.FontWeight = font.ToFontWeight();
		}

		public static void UpdateIsEnabled(this Control nativeControl, bool isEnabled) =>
			nativeControl.IsEnabled = isEnabled;

		public static void UpdateBackground(this Control nativeControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateProperty(Control.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToNative());

		public static void UpdateBackground(this Border nativeControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateProperty(Border.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToNative());

		public static void UpdateBackground(this Panel nativeControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateProperty(Panel.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToNative());

		public static void UpdateForegroundColor(this Control nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null)
			=> nativeControl.Foreground = color?.ToNative() ?? defaultBrush ?? nativeControl.Foreground;

		public static void UpdatePadding(this Control nativeControl, Thickness padding, UI.Xaml.Thickness? defaultThickness = null)
		{
			// TODO: have a way to reset the padding
			//       This is used for button, but this also means there can never be a 0 padding button
			var newPadding = defaultThickness ?? new UI.Xaml.Thickness();

			newPadding.Left += padding.Left;
			newPadding.Top += padding.Top;
			newPadding.Right += padding.Right;
			newPadding.Bottom += padding.Bottom;

			nativeControl.Padding = newPadding;
		}
	}
}
