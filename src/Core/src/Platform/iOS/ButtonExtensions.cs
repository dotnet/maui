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
		}

		public static void UpdatePadding(this UIButton platformButton, IButton button, Thickness? defaultPadding = null) =>
			UpdatePadding(platformButton, button.Padding, defaultPadding);

		public static void UpdatePadding(this UIButton platformButton, Thickness padding, Thickness? defaultPadding = null)
		{
			if (padding.IsNaN)
				padding = defaultPadding ?? Thickness.Zero;

			int additionalPadding = (int)platformButton.Layer.BorderWidth;
			padding = new Thickness(padding.Left + additionalPadding, padding.Top + additionalPadding, padding.Right + additionalPadding, padding.Bottom + additionalPadding);

			// top and bottom insets reset to a "default" if they are exactly 0
			// however, internally they are floor-ed, so there is no actual fractions
			var top = padding.Top;
			if (top == 0.0)
				top = AlmostZero;
			var bottom = padding.Bottom;
			if (bottom == 0.0)
				bottom = AlmostZero;

			// The downside of using the ContentEdgeInsets is that in non-UIButtonConfiguration instances, it will truncate the title for buttons with images on top or bottom more than necessary.
#pragma warning disable CA1416 // TODO: 'UIButton.ContentEdgeInsets' is unsupported on: 'ios' 15.0 and later.
#pragma warning disable CA1422 // Validate platform compatibility
			platformButton.ContentEdgeInsets = new UIEdgeInsets(
				(float)top,
				(float)padding.Left,
				(float)bottom,
				(float)padding.Right);
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
		}

		internal static void UpdateContentEdgeInsets(this UIButton platformButton, IButton button, Thickness? defaultPadding = null) =>
			UpdateContentEdgeInsets(platformButton, button.Padding, defaultPadding);

		internal static void UpdateContentEdgeInsets(this UIButton platformButton, Thickness padding, Thickness? defaultPadding = null)
		{
			// top and bottom insets reset to a "default" if they are exactly 0
			// however, internally they are floor-ed, so there is no actual fractions
			var top = padding.Top;
			if (top == 0.0)
				top = AlmostZero;
			var bottom = padding.Bottom;
			if (bottom == 0.0)
				bottom = AlmostZero;

#pragma warning disable CA1416 // TODO: 'UIButton.ContentEdgeInsets' is unsupported on: 'ios' 15.0 and later.
#pragma warning disable CA1422 // Validate platform compatibility
			platformButton.ContentEdgeInsets = new UIEdgeInsets(
				(float)top,
				(float)padding.Left,
				(float)bottom,
				(float)padding.Right);
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
		}
	}
}