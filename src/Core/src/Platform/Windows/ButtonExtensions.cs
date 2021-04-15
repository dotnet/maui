#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateBackgroundColor(this Button nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateBackgroundColor(button.BackgroundColor, defaultBrush);

		public static void UpdateText(this Button nativeButton, IButton button) =>
			nativeButton.Content = button.Text;

		public static void UpdateTextColor(this Button nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateForegroundColor(button.TextColor, defaultBrush);

		public static void UpdatePadding(this Button nativeButton, IButton button, UI.Xaml.Thickness? defaultThickness = null) =>
			nativeButton.UpdatePadding(button.Padding, defaultThickness);

		public static void UpdateFont(this Button nativeButton, IButton button, IFontManager fontManager) =>
			nativeButton.UpdateFont(button.Font, fontManager);
	}
}