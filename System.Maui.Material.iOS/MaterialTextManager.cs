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
using System.Maui.Internals;
using System.Maui.Platform.iOS;

namespace System.Maui.Material.iOS
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