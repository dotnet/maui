#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class ControlExtensions
	{
		public static void UpdateFont(this Control nativeButton, ITextStyle textStyle, IFontManager fontManager) =>
			nativeButton.UpdateFont(textStyle.Font, fontManager);

		public static void UpdateFont(this Control nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.ToFontStyle();
			nativeControl.FontWeight = font.ToFontWeight();
			nativeControl.IsTextScaleFactorEnabled = font.AutoScalingEnabled;
		}

		public static void UpdateIsEnabled(this Control nativeControl, bool isEnabled) =>
			nativeControl.IsEnabled = isEnabled;

		public static void UpdateBackground(this Control nativeControl, IView view, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateBackground(view.Background, defaultBrush);

		public static void UpdateBackground(this Control nativeControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateProperty(Control.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToNative());

		public static void UpdateBackground(this Border nativeControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateProperty(Border.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToNative());

		public static void UpdateBackground(this Panel nativeControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.UpdateProperty(Panel.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToNative());

		public static void UpdateForegroundColor(this Control nativeControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeControl.Foreground = color?.ToNative() ?? defaultBrush ?? nativeControl.Foreground;

		public static void UpdatePadding(this Control nativeControl, IPadding padding, UI.Xaml.Thickness? defaultThickness = null) =>
			nativeControl.UpdatePadding(padding.Padding, defaultThickness);

		public static void UpdatePadding(this Control nativeControl, Thickness padding, UI.Xaml.Thickness? defaultThickness = null)
		{
			nativeControl.Padding = padding.IsNaN
				? defaultThickness ?? new UI.Xaml.Thickness()
				: padding.ToNative();
		}
	}
}