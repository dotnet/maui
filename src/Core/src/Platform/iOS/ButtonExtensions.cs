using System;
using Foundation;
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

		public static void UpdateText(this UIButton platformButton, IText button) 
		{
			platformButton.SetTitle(button.Text, UIControlState.Normal);

			if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is UIButtonConfiguration config)
			{
				config.Title = button.Text;
				platformButton.Configuration = config;
			}

			UpdateCharacterSpacing(platformButton, button);
		}

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
			object? config = OperatingSystem.IsIOSVersionAtLeast(15) ? platformButton.Configuration : null;

			// This is probalby wrong and needs to be tested more
			NSAttributedString? nSAttributedString = (config as UIButtonConfiguration)?.AttributedTitle ?? platformButton.TitleLabel?.AttributedText;

			if (nSAttributedString is null && textStyle is IText text && text.Text is not null)
			{
				nSAttributedString = new NSAttributedString(text.Text);
			}
			else if (nSAttributedString is null)
			{
				return;
			}

			var attributedText = nSAttributedString.WithCharacterSpacing(textStyle.CharacterSpacing);
			if (textStyle.TextColor != null)
				attributedText = attributedText?.WithTextColor(textStyle.TextColor);

			if (config is UIButtonConfiguration buttonConfig)
			{
				buttonConfig.AttributedTitle = attributedText;
				platformButton.Configuration = buttonConfig;
			}
			else
			{
				platformButton.SetAttributedTitle(attributedText, UIControlState.Normal);
			}
		}

		public static void UpdateFont(this UIButton platformButton, ITextStyle textStyle, IFontManager fontManager)
		{
			if (platformButton.TitleLabel is not null)
				platformButton.TitleLabel.UpdateFont(textStyle, fontManager, UIFont.ButtonFontSize);

			// If iOS 15+, update the configuration with the new font
			if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is UIButtonConfiguration config && textStyle is IText text)
			{
				var attributedText = config.AttributedTitle ?? platformButton.TitleLabel?.AttributedText ??
					new NSAttributedString (text.Text);

				var newAttributedText = attributedText.WithFont(fontManager.GetFont(textStyle.Font, textStyle.Font.Size));

				// TODO: this should get saved from the configuration but is not currently so we will use teh SetAttributedTitle for now.
				platformButton.SetAttributedTitle(newAttributedText, UIControlState.Normal);
				config.AttributedTitle = newAttributedText;
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

			if (OperatingSystem.IsIOSVersionAtLeast(15) && platformButton.Configuration is UIButtonConfiguration config)
			{
				config.ContentInsets = new NSDirectionalEdgeInsets (
					(float)top,
					(float)padding.Left,
					(float)bottom,
					(float)padding.Right);
				platformButton.Configuration = config;
			}
			else
			{
				// ImageButton still will use the deprecated UIEdgeInsets for now.
#pragma warning disable CA1422 // Validate platform compatibility
				platformButton.ContentEdgeInsets = new UIEdgeInsets(
					(float)top,
					(float)padding.Left,
					(float)bottom,
					(float)padding.Right);
#pragma warning restore CA1422 // Validate platform compatibility
			}
		}
	}
}