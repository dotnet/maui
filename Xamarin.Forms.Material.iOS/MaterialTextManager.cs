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

			var textColor = MaterialColors.GetEntryTextColor(element.TextColor);
			var placeHolderColors = MaterialColors.GetPlaceHolderColor(element.PlaceholderColor, element.TextColor);
			var underlineColors = MaterialColors.GetUnderlineColor(element.PlaceholderColor);

			textField.TextInput.TextColor = textColor;
			textField.ActiveTextInputController.InlinePlaceholderColor = placeHolderColors.InlineColor;
			textField.ActiveTextInputController.FloatingPlaceholderNormalColor = placeHolderColors.InlineColor;
			textField.ActiveTextInputController.FloatingPlaceholderActiveColor = placeHolderColors.FloatingColor;

			var brush = element.Background;

			if (Brush.IsNullOrEmpty(brush))
			{
				// BackgroundColor
				textField.ActiveTextInputController.BorderFillColor = MaterialColors.CreateEntryFilledInputBackgroundColor(element.BackgroundColor, element.TextColor);
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
			var uIColor = MaterialColors.GetEntryTextColor(element.TextColor);
			textField.ContainerScheme.ColorScheme.OnSurfaceColor = uIColor;
			textField.ContainerScheme.ColorScheme.PrimaryColor = uIColor;
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