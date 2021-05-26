#nullable enable
namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateBackground(this MauiButton nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateBackground(button.Background, defaultBrush);

		public static void UpdateText(this MauiButton nativeButton, IButton button) =>
			nativeButton.Content = button.Text;

		public static void UpdateTextColor(this MauiButton nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateForegroundColor(button.TextColor, defaultBrush);

		public static void UpdateCharacterSpacing(this MauiButton nativeButton, IButton button) =>
			nativeButton.UpdateCharacterSpacing((int)button.CharacterSpacing);

		public static void UpdatePadding(this MauiButton nativeButton, IButton button, UI.Xaml.Thickness? defaultThickness = null) =>
			nativeButton.UpdatePadding(button.Padding, defaultThickness);

		public static void UpdateFont(this MauiButton nativeButton, IButton button, IFontManager fontManager) =>
			nativeButton.UpdateFont(button.Font, fontManager);
	}
}