using System;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;
using MTextInputControllerFilled = MaterialComponents.TextInputControllerFilled;

namespace Xamarin.Forms.Material.iOS
{
	internal static class MaterialTextManager
	{
		static double AlphaAdjustment = 0.0;

		public static void Init(IMaterialEntryRenderer element, IMaterialTextField textField, IFontElement fontElement)
		{
			var containerScheme = textField.ContainerScheme;
			textField.TextInput.ClearButtonMode = UITextFieldViewMode.Never;
			textField.ActiveTextInputController = new MTextInputControllerFilled(textField.TextInput);
			textField.TextInput.TextInsetsMode = TextInputTextInsetsMode.IfContent;
			containerScheme.TypographyScheme = CreateTypographyScheme();
			containerScheme.ColorScheme = (SemanticColorScheme)CreateColorScheme();
			ApplyTypographyScheme(textField, fontElement);
			ApplyTheme(textField, element);
		}

		public static void ApplyTypographyScheme(IMaterialTextField textField, IFontElement fontElement)
		{
			var containerScheme = textField.ContainerScheme;
			textField.TextInput.Font = fontElement?.ToUIFont();
			containerScheme.TypographyScheme.Subtitle1 = textField.TextInput.Font;
			ApplyContainerTheme(textField);
		}

		static void ApplyContainerTheme(IMaterialTextField textField)
		{
			var containerScheme = textField.ContainerScheme;

			if (textField.ActiveTextInputController is MTextInputControllerFilled filled)
				filled.ApplyTheme(containerScheme);
		}

		public static void ApplyTheme(IMaterialTextField textField, IMaterialEntryRenderer element)
		{
			if (element == null)
				return;

			if (textField.ActiveTextInputController == null)
				return;

			textField.ContainerScheme.ColorScheme = (SemanticColorScheme)CreateColorScheme();
			ApplyContainerTheme(textField);

			var adjustedTextColor = AdjustTextColor(element);

			var textColor = MaterialColors.GetEntryTextColor(adjustedTextColor);
			var placeHolderColors = MaterialColors.GetPlaceHolderColor(element.PlaceholderColor, adjustedTextColor);
			var underlineColors = MaterialColors.GetUnderlineColor(element.PlaceholderColor);

			textField.TextInput.TextColor = textColor;

			var inputController = textField.ActiveTextInputController;
			inputController.InlinePlaceholderColor = placeHolderColors.InlineColor;
			inputController.FloatingPlaceholderNormalColor = placeHolderColors.InlineColor;
			inputController.FloatingPlaceholderActiveColor = placeHolderColors.FloatingColor;
			inputController.DisabledColor = placeHolderColors.InlineColor;

			var brush = element.Background;

			if (Brush.IsNullOrEmpty(brush))
			{
				// BackgroundColor
				textField.ActiveTextInputController.BorderFillColor = MaterialColors.CreateEntryFilledInputBackgroundColor(element.BackgroundColor, adjustedTextColor);
			}
			else
			{
				// Background
				if (textField is UITextField || textField is MultilineTextField)
				{
					var backgroundImage = ((UIView)textField).GetBackgroundImage(brush);
					textField.BackgroundSize = backgroundImage?.Size;
					var color = backgroundImage != null ? UIColor.FromPatternImage(backgroundImage) : UIColor.Clear;
					textField.ActiveTextInputController.BorderFillColor = color;
				}
			}
			textField.ActiveTextInputController.ActiveColor = underlineColors.FocusedColor;
			textField.ActiveTextInputController.NormalColor = underlineColors.UnFocusedColor;
		}

		public static void ApplyThemeIfNeeded(IMaterialTextField textField, IMaterialEntryRenderer element)
		{
			var bgBrush = element.Background;

			if (Brush.IsNullOrEmpty(bgBrush))
				return;

			UIImage backgroundImage = null;

			if (textField is UITextField || textField is MultilineTextField)
				backgroundImage = ((UIView)textField).GetBackgroundImage(bgBrush);

			if (textField.BackgroundSize != null && textField.BackgroundSize != backgroundImage?.Size)
				ApplyTheme(textField, element);
		}

		public static void UpdatePlaceholder(IMaterialTextField textField, IMaterialEntryRenderer element)
		{
			var placeholderText = element.Placeholder ?? String.Empty;
			textField.ActiveTextInputController.PlaceholderText = placeholderText;
			ApplyTheme(textField, element);

			var previous = textField.ActiveTextInputController.FloatingPlaceholderScale;
			if (String.IsNullOrWhiteSpace(placeholderText))
				textField.ActiveTextInputController.FloatingPlaceholderScale = 0;
			else
				textField.ActiveTextInputController.FloatingPlaceholderScale = (float)TextInputControllerBase.FloatingPlaceholderScaleDefault;

			if (previous != textField.ActiveTextInputController.FloatingPlaceholderScale && element is IVisualElementRenderer controller)
				controller.Element?.InvalidateMeasureInternal(InvalidationTrigger.VerticalOptionsChanged);
		}

		public static void UpdateTextColor(IMaterialTextField textField, IMaterialEntryRenderer element)
		{
			var adjustedTextColor = AdjustTextColor(element);

			var uIColor = MaterialColors.GetEntryTextColor(adjustedTextColor);
			textField.ContainerScheme.ColorScheme.OnSurfaceColor = uIColor;
			textField.ContainerScheme.ColorScheme.PrimaryColor = uIColor;
		}

		static Color AdjustTextColor(IMaterialEntryRenderer element) 
		{
			if (Forms.IsiOS14OrNewer)
			{
				// This is a workaround for an iOS/Material bug; https://github.com/xamarin/Xamarin.Forms/issues/12246
				// If we are on iOS 14, and we have multiple material text entry fields of the same color,
				// and any of them are password fields, setting them to the same TextColor value will cause the application
				// to hang when a password field loses focus. 

				// So to work around this, we make an imperceptible adjustment to the alpha value of the color each time
				// we set it; that way, none of the text entry fields have _exactly_ the same color and we avoid the bug

				// Obviously this will start to become noticeable after the first 20 million or so text entry fields are displayed.
				// We apologize for the inconvenience.

				var elementTextColor = element.TextColor;
				AlphaAdjustment += 0.0000001;

				var adjustedAlpha = elementTextColor.IsDefault ? 1 - AlphaAdjustment : elementTextColor.A - AlphaAdjustment;
				if (adjustedAlpha < 0)
				{
					// Below an alpha of 0.01 stuff on iOS doesn't show up in hit tests anyway, so it seems unlikely
					// that the entry will get focus and cause the issue. 
					adjustedAlpha = 0;
				}

				return new Color(elementTextColor.R, elementTextColor.G, elementTextColor.B, adjustedAlpha);
			}

			return element.TextColor;
		}

		static IColorScheming CreateColorScheme()
		{
			var returnValue = MaterialColors.Light.CreateColorScheme();
			return returnValue;
		}

		static TypographyScheme CreateTypographyScheme()
		{
			return new TypographyScheme();
		}
	}
}