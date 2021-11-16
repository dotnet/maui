using System;
using UIKit;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		static float DefaultCornerRadius = 5;

		public static void UpdateBorderColor(this UIButton nativeButton, IButtonBorder buttonBorder)
		{
			if (buttonBorder.BorderColor != null)
				nativeButton.Layer.BorderColor = buttonBorder.BorderColor.ToCGColor();
		}

		public static void UpdateBorderWidth(this UIButton nativeButton, IButtonBorder buttonBorder)
		{
			nativeButton.Layer.BorderWidth = Math.Max(0f, (float)buttonBorder.BorderWidth);
		}

		public static void UpdateCornerRadius(this UIButton nativeButton, IButtonBorder buttonBorder)
		{
			var cornerRadius = DefaultCornerRadius;

			if (cornerRadius != buttonBorder.CornerRadius)
				cornerRadius = buttonBorder.CornerRadius;

			nativeButton.Layer.CornerRadius = cornerRadius;
		}

		public static void UpdateText(this UIButton nativeButton, IText button) =>
			nativeButton.SetTitle(button.Text, UIControlState.Normal);

		public static void UpdateTextColor(this UIButton nativeButton, ITextStyle button) =>
			nativeButton.UpdateTextColor(button);

		public static void UpdateTextColor(this UIButton nativeButton, ITextStyle button, UIColor? buttonTextColorDefaultNormal, UIColor? buttonTextColorDefaultHighlighted, UIColor? buttonTextColorDefaultDisabled)
		{
			if (button.TextColor == null)
			{
				nativeButton.SetTitleColor(buttonTextColorDefaultNormal, UIControlState.Normal);
				nativeButton.SetTitleColor(buttonTextColorDefaultHighlighted, UIControlState.Highlighted);
				nativeButton.SetTitleColor(buttonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				var color = button.TextColor.ToNative();

				nativeButton.SetTitleColor(color, UIControlState.Normal);
				nativeButton.SetTitleColor(color, UIControlState.Highlighted);
				nativeButton.SetTitleColor(color, UIControlState.Disabled);

				nativeButton.TintColor = color;
			}
		}

		public static void UpdateCharacterSpacing(this UIButton nativeButton, ITextStyle textStyle)
		{
			nativeButton.TitleLabel.UpdateCharacterSpacing(textStyle);
		}

		public static void UpdateFont(this UIButton nativeButton, ITextStyle textStyle, IFontManager fontManager)
		{
			nativeButton.TitleLabel.UpdateFont(textStyle, fontManager, UIFont.ButtonFontSize);
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