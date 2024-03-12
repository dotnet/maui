using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public const double AlmostZero = 0.00001;

		public static void UpdateStrokeColor(this UIButton platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeColor is not null)
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

		public static void UpdateTextColor(this UIButton platformButton, ITextStyle button)
		{
			if (button.TextColor is null)
				return;

			var color = button.TextColor.ToPlatform();

			platformButton.SetTitleColor(color, UIControlState.Normal);
			platformButton.SetTitleColor(color, UIControlState.Highlighted);
			platformButton.SetTitleColor(color, UIControlState.Disabled);

			platformButton.TintColor = color;
		}

		public static void UpdateCharacterSpacing(this UIButton platformButton, ITextStyle textStyle)
		{
			var attributedText = platformButton?.TitleLabel.AttributedText?.WithCharacterSpacing(textStyle.CharacterSpacing);
			if (textStyle.TextColor != null)
				attributedText = attributedText?.WithTextColor(textStyle.TextColor);

			platformButton?.SetAttributedTitle(attributedText, UIControlState.Normal);
		}

		public static void UpdateFont(this UIButton platformButton, ITextStyle textStyle, IFontManager fontManager)
		{
			platformButton.TitleLabel.UpdateFont(textStyle, fontManager, UIFont.ButtonFontSize);

			// If iOS 15+, update the configuration with the new font
			if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is UIButtonConfiguration config)
			{
				config.TitleTextAttributesTransformer = (incoming) =>
				{
					var outgoing = incoming;
					outgoing[UIStringAttributeKey.Font] = platformButton.TitleLabel.Font;
					return outgoing;
				};
				platformButton.Configuration = config;
			}
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

			if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is not null)
			{
				var config = platformButton.Configuration;
				config.ContentInsets = new NSDirectionalEdgeInsets(
					(float)top,
					(float)padding.Left,
					(float)bottom,
					(float)padding.Right);
				platformButton.Configuration = config;
			}
			else if (!OperatingSystem.IsIOSVersionAtLeast(15))
			{
				platformButton.ContentEdgeInsets = new UIEdgeInsets(
					(float)top,
					(float)padding.Left,
					(float)bottom,
					(float)padding.Right);
			}
		}
	}
}