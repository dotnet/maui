#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateBackground(this Button nativeButton, IButton button) =>
			nativeButton.UpdateBackground(button.Background);

		public static void UpdateText(this Button nativeButton, IButton button) =>
			nativeButton.Content = button.Text;

		public static void UpdateForeground(this Button nativeButton, IButton button) =>
			nativeButton.UpdateForeground(button.Foreground);

		public static void UpdatePadding(this Button nativeButton, IButton button, UI.Xaml.Thickness? defaultThickness = null) =>
			nativeButton.UpdatePadding(button.Padding, defaultThickness);

		public static void UpdateFont(this Button nativeButton, IButton button, IFontManager fontManager) =>
			nativeButton.UpdateFont(button.Font, fontManager);
	}
}