#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class ControlExtensions
	{
		public static void UpdateFont(this Control platformButton, ITextStyle textStyle, IFontManager fontManager) =>
			platformButton.UpdateFont(textStyle.Font, fontManager);

		public static void UpdateFont(this Control platformControl, Font font, IFontManager fontManager)
		{
			platformControl.FontSize = fontManager.GetFontSize(font);
			platformControl.FontFamily = fontManager.GetFontFamily(font);
			platformControl.FontStyle = font.ToFontStyle();
			platformControl.FontWeight = font.ToFontWeight();
			platformControl.IsTextScaleFactorEnabled = font.AutoScalingEnabled;
		}

		public static void UpdateIsEnabled(this Control platformControl, bool isEnabled) =>
			platformControl.IsEnabled = isEnabled;

		public static void UpdateBackground(this Control platformControl, IView view, UI.Xaml.Media.Brush? defaultBrush = null) =>
			platformControl.UpdateBackground(view.Background, defaultBrush);

		public static void UpdateBackground(this Control platformControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			platformControl.UpdateProperty(Control.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToPlatform());

		public static void UpdateBackground(this Border platformControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			platformControl.UpdateProperty(Border.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToPlatform());

		public static void UpdateBackground(this Panel platformControl, Paint? paint, UI.Xaml.Media.Brush? defaultBrush = null) =>
			platformControl.UpdateProperty(Panel.BackgroundProperty, paint.IsNullOrEmpty() ? defaultBrush : paint?.ToPlatform());

		public static void UpdateForegroundColor(this Control platformControl, Color color, UI.Xaml.Media.Brush? defaultBrush = null) =>
			platformControl.Foreground = color?.ToPlatform() ?? defaultBrush ?? platformControl.Foreground;

		public static void UpdatePadding(this Control platformControl, IPadding padding, UI.Xaml.Thickness? defaultThickness = null) =>
			platformControl.UpdatePadding(padding.Padding, defaultThickness);

		public static void UpdatePadding(this Control platformControl, Thickness padding, UI.Xaml.Thickness? defaultThickness = null)
		{
			platformControl.Padding = padding.IsNaN
				? defaultThickness ?? new UI.Xaml.Thickness()
				: padding.ToPlatform();
		}
	}
}