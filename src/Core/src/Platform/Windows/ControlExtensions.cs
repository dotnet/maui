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
			nativeControl.FontStyle = font.FontAttributes.ToFontStyle();
			nativeControl.FontWeight = font.FontAttributes.ToFontWeight();
		}

		public static void UpdateIsEnabled(this Control nativeControl, bool isEnabled) =>
			nativeControl.IsEnabled = isEnabled;

		public static void UpdateForegroundColor(this Control nativeControl, Color color) =>
			nativeControl.UpdateProperty(Control.ForegroundProperty, color);

		public static void UpdateBackground(this Control nativeControl, IBrush? brush) =>
			nativeControl.UpdateProperty(Control.BackgroundProperty, brush?.ToNative());

		public static void UpdateBackground(this Border nativeControl, IBrush? brush) =>
			nativeControl.UpdateProperty(Border.BackgroundProperty, brush?.ToNative());

		public static void UpdateBackground(this Panel nativeControl, IBrush? brush) =>
			nativeControl.UpdateProperty(Panel.BackgroundProperty, brush?.ToNative());

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