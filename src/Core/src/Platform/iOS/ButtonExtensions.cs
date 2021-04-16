using UIKit;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this UIButton nativeButton, IButton button) =>
			nativeButton.SetTitle(button.Text, UIControlState.Normal);

		public static void UpdateForeground(this UIButton nativeButton, IButton button) =>
			nativeButton.UpdateForeground(button);

		public static void UpdateForeground(this UIButton nativeButton, IButton button, UIColor? buttonTextColorDefaultNormal, UIColor? buttonTextColorDefaultHighlighted, UIColor? buttonTextColorDefaultDisabled)
		{
			var foreground = button.Foreground;

			nativeButton.SetForeground(foreground, buttonTextColorDefaultNormal, buttonTextColorDefaultHighlighted, buttonTextColorDefaultDisabled);
		}

		public static void UpdateCharacterSpacing(this UIButton nativeButton, ITextStyle textStyle)
		{
			nativeButton.TitleLabel.UpdateCharacterSpacing(textStyle);
		}

		public static void UpdateFont(this UIButton nativeButton, ITextStyle textStyle, IFontManager fontManager)
		{
			nativeButton.TitleLabel.UpdateFont(textStyle, fontManager);
		}

		public static void UpdatePadding(this UIButton nativeButton, IButton button)
		{
			nativeButton.ContentEdgeInsets = new UIEdgeInsets(
				(float)button.Padding.Top,
				(float)button.Padding.Left,
				(float)button.Padding.Bottom,
				(float)button.Padding.Right);
		}
	}
}