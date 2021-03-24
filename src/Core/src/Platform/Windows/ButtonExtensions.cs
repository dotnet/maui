using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this Button nativeButton, IButton button) =>
			nativeButton.Content = button.Text;

		public static void UpdateFont(this Button nativeButton, IButton button, IFontManager fontManager) =>
			nativeButton.UpdateFont(button.Font, fontManager);
	}
}