#nullable enable
namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this MauiButton nativeButton, IText button) =>
			nativeButton.Content = button.Text;

		public static void UpdateTextColor(this MauiButton nativeButton, ITextStyle button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateForegroundColor(button.TextColor, defaultBrush);

		public static void UpdateCharacterSpacing(this MauiButton nativeButton, ITextStyle button) =>
			nativeButton.UpdateCharacterSpacing((int)button.CharacterSpacing);
	}
}