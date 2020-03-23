using System;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using MTextField = MaterialComponents.TextField;
using MTextInputControllerFilled = MaterialComponents.TextInputControllerFilled;
using MTextInputControllerBase = MaterialComponents.TextInputControllerBase;
using System.Collections.Generic;
using ObjCRuntime;
using Foundation;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms.Material.iOS
{
	internal static class MaterialTextManager
	{
		public static void Init(IMaterialEntryRenderer element, IMaterialTextField textField, IFontElement fontElement)
		{
			textField.TextInput.ClearButtonMode = UITextFieldViewMode.Never;
			textField.ActiveTextInputController = new MTextInputControllerFilled(textField.TextInput);
			textField.TextInput.TextInsetsMode = TextInputTextInsetsMode.IfContent;
			textField.TypographyScheme = CreateTypographyScheme();
			textField.ColorScheme = (SemanticColorScheme)CreateColorScheme();
			ApplyTypographyScheme(textField, fontElement);
			ApplyTheme(textField, element);
		}
		public static void ApplyTypographyScheme(IMaterialTextField textField, IFontElement fontElement)
		{
			textField.TextInput.Font = fontElement?.ToUIFont();
			textField.TypographyScheme.Subtitle1 = textField.TextInput.Font;
			TextFieldTypographyThemer.ApplyTypographyScheme(textField.TypographyScheme, textField.TextInput);
			TextFieldTypographyThemer.ApplyTypographyScheme(textField.TypographyScheme, textField.ActiveTextInputController);
		}

		public static void ApplyTheme(IMaterialTextField textField, IMaterialEntryRenderer element)
		{
			if (element == null)
				return;

			if (textField.ActiveTextInputController == null)
				return;

			FilledTextFieldColorThemer.ApplySemanticColorScheme(textField.ColorScheme, (MTextInputControllerFilled)textField.ActiveTextInputController);

			var textColor = MaterialColors.GetEntryTextColor(element.TextColor);
			var placeHolderColors = MaterialColors.GetPlaceHolderColor(element.PlaceholderColor, element.TextColor);
			var underlineColors = MaterialColors.GetUnderlineColor(element.PlaceholderColor);

			textField.TextInput.TextColor = textColor;
			textField.ActiveTextInputController.InlinePlaceholderColor = placeHolderColors.InlineColor;
			textField.ActiveTextInputController.FloatingPlaceholderNormalColor = placeHolderColors.InlineColor;
			textField.ActiveTextInputController.FloatingPlaceholderActiveColor = placeHolderColors.FloatingColor;

			// BackgroundColor
			textField.ActiveTextInputController.BorderFillColor = MaterialColors.CreateEntryFilledInputBackgroundColor(element.BackgroundColor, element.TextColor);

			textField.ActiveTextInputController.ActiveColor = underlineColors.FocusedColor;
			textField.ActiveTextInputController.NormalColor = underlineColors.UnFocusedColor;
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
			textField.ColorScheme.OnSurfaceColor = uIColor;
			textField.ColorScheme.PrimaryColor = uIColor;
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