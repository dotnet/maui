#nullable enable
using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateBackgroundColor(this Button nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateBackgroundColor(button.BackgroundColor, defaultBrush);

		public static void UpdateText(this MauiButton nativeButton, IButton button) =>
			nativeButton.Content = button.Text;

		public static void UpdateTextColor(this MauiButton nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateForegroundColor(button.TextColor, defaultBrush);

		public static void UpdatePadding(this MauiButton nativeButton, IButton button, UI.Xaml.Thickness? defaultThickness = null) =>
			nativeButton.UpdatePadding(button.Padding, defaultThickness);

		public static void UpdateFont(this MauiButton nativeButton, IButton button, IFontManager fontManager) =>
			nativeButton.UpdateFont(button.Font, fontManager);

		public static void UpdateCharacterSpacing(this MauiButton nativeButton, IButton button) =>
			nativeButton.CharacterSpacing = button.CharacterSpacing.ToEm();

		[PortHandler("LineBreakMode is not implemented yet. Enable commented line after the implementation.")]
		public static void UpdateLineBreakMode(this MauiButton nativeButton, IButton button)
		{
			// nativeButton.LineBreakMode = button.LineBreakMode;
		}

		public static void UpdateCornerRadius(this MauiButton nativeButton, IButton button)
		{
			var validRadius = Math.Max(0, button.CornerRadius);
			var cornerRadius = WinUIHelpers.CreateCornerRadius(validRadius);

			nativeButton.CornerRadius = cornerRadius;
		}
	}
}