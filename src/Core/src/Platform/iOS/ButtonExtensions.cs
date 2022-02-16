using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public const double AlmostZero = 0.00001;

		public static void UpdateStrokeColor(this UIButton platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeColor != null)
				platformButton.Layer.BorderColor = buttonStroke.StrokeColor.ToCGColor();
		}

		public static void UpdateStrokeThickness(this UIButton platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeThickness >= 0)
				platformButton.Layer.BorderWidth = (float)buttonStroke.StrokeThickness;
		}

		public static void UpdateCornerRadius(this UIButton platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.CornerRadius >= 0)
				platformButton.Layer.CornerRadius = buttonStroke.CornerRadius;
		}

		public static void UpdateText(this UIButton platformButton, IText button) =>
			platformButton.SetTitle(button.Text, UIControlState.Normal);

		public static void UpdateTextColor(this UIButton platformButton, ITextStyle button) =>
			platformButton.UpdateTextColor(button);

		public static void UpdateTextColor(this UIButton platformButton, ITextStyle button, UIColor? buttonTextColorDefaultNormal, UIColor? buttonTextColorDefaultHighlighted, UIColor? buttonTextColorDefaultDisabled)
		{
			if (button.TextColor == null)
			{
				platformButton.SetTitleColor(buttonTextColorDefaultNormal, UIControlState.Normal);
				platformButton.SetTitleColor(buttonTextColorDefaultHighlighted, UIControlState.Highlighted);
				platformButton.SetTitleColor(buttonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				var color = button.TextColor.ToPlatform();

				platformButton.SetTitleColor(color, UIControlState.Normal);
				platformButton.SetTitleColor(color, UIControlState.Highlighted);
				platformButton.SetTitleColor(color, UIControlState.Disabled);

				platformButton.TintColor = color;
			}
		}

		public static void UpdateCharacterSpacing(this UIButton platformButton, ITextStyle textStyle)
		{
			platformButton.TitleLabel.UpdateCharacterSpacing(textStyle);
		}

		public static void UpdateFont(this UIButton platformButton, ITextStyle textStyle, IFontManager fontManager)
		{
			platformButton.TitleLabel.UpdateFont(textStyle, fontManager, UIFont.ButtonFontSize);
		}

		public static void UpdatePadding(this UIButton platformButton, IButton button, Thickness? defaultPadding = null) =>
			UpdatePadding(platformButton, button.Padding, defaultPadding);

		public static void UpdatePadding(this UIButton platformButton, Thickness padding, Thickness? defaultPadding = null)
		{
			if (padding.IsNaN)
				padding = defaultPadding ?? Thickness.Zero;

			// top and bottom insets reset to a "default" if they are exactly 0
			// however, internally they are floor-ed, so there is no actual fractions
			var top = padding.Top;
			if (top == 0.0)
				top = AlmostZero;
			var bottom = padding.Bottom;
			if (bottom == 0.0)
				bottom = AlmostZero;

			platformButton.ContentEdgeInsets = new UIEdgeInsets(
				(float)top,
				(float)padding.Left,
				(float)bottom,
				(float)padding.Right);
		}
	}
}