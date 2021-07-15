#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WCornerRadius = Microsoft.UI.Xaml.CornerRadius;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateBackground(this Button nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null)
		{
			var paint = button.Background;
			var brush = paint.IsNullOrEmpty() ? defaultBrush : paint?.ToNative();

			if (brush != null)
			{
				nativeButton.Resources["ButtonBackground"] = brush;
				nativeButton.Resources["ButtonBackgroundPointerOver"] = brush;
				nativeButton.Resources["ButtonBackgroundPressed"] = brush;
				nativeButton.Resources["ButtonBackgroundDisabled"] = brush;
			}
		}

		public static void UpdateText(this Button nativeButton, IButton button) =>
			nativeButton.Content = button.Text;

		public static void UpdateCornerRadius(this Button nativeButton, IButton button)
		{
			if (button.CornerRadius.ToNative() is WCornerRadius radius)
			{
				nativeButton.Resources["ControlCornerRadius"] = radius;
				nativeButton.CornerRadius = radius;
			}
		}

		public static void UpdateTextColor(this Button nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null)
		{
			var color = button.TextColor;
			var brush = color.IsDefault() ? defaultBrush : color?.ToNative();

			if (brush != null)
			{
				nativeButton.Resources["ButtonForeground"] = brush;
				nativeButton.Resources["ButtonForegroundPointerOver"] = brush;
				nativeButton.Resources["ButtonForegroundPressed"] = brush;
				nativeButton.Resources["ButtonForegroundDisabled"] = brush;
			}
		}

		public static void UpdateCharacterSpacing(this Button nativeButton, IButton button) =>
			nativeButton.CharacterSpacing = button.CharacterSpacing.ToEm();

		public static void UpdatePadding(this Button nativeButton, IButton button, UI.Xaml.Thickness? defaultThickness = null) =>
			nativeButton.UpdatePadding(button.Padding, defaultThickness);

		public static void UpdateFont(this Button nativeButton, IButton button, IFontManager fontManager) =>
			nativeButton.UpdateFont(button.Font, fontManager);
	}
}