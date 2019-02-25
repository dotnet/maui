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

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialTextField : MTextField
	{
		SemanticColorScheme _colorScheme;
		TypographyScheme _typographyScheme;
		MTextInputControllerBase _activeTextinputController;

		public MaterialTextField(IMaterialEntryRenderer element, IFontElement fontElement)
		{
			VisualElement.VerifyVisualFlagEnabled();
			ClearButtonMode = UITextFieldViewMode.Never;
			_activeTextinputController = new MTextInputControllerFilled(this);
			TextInsetsMode = TextInputTextInsetsMode.IfContent;
			_typographyScheme = CreateTypographyScheme();
			_colorScheme = (SemanticColorScheme)CreateColorScheme();
			ApplyTypographyScheme(fontElement);
			ApplyTheme(element);

		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var result = base.SizeThatFits(size);

			if (nfloat.IsInfinity(result.Width))
				result = SystemLayoutSizeFittingSize(result, (float)UILayoutPriority.FittingSizeLevel, (float)UILayoutPriority.DefaultHigh);

			return result;
		}

		internal void ApplyTypographyScheme(IFontElement fontElement)
		{
			Font = fontElement?.ToUIFont();
			_typographyScheme.Subtitle1 = Font;
			TextFieldTypographyThemer.ApplyTypographyScheme(_typographyScheme, this);
			TextFieldTypographyThemer.ApplyTypographyScheme(_typographyScheme, _activeTextinputController);
		}

		internal void ApplyTheme(IMaterialEntryRenderer element)
		{
			if (element == null)
				return;

			if (_activeTextinputController == null)
				return;

			FilledTextFieldColorThemer.ApplySemanticColorScheme(_colorScheme, (MTextInputControllerFilled)_activeTextinputController);

			var textColor = MaterialColors.GetEntryTextColor(element.TextColor);
			var placeHolderColors = MaterialColors.GetPlaceHolderColor(element.PlaceholderColor, element.TextColor);
			var underlineColors = MaterialColors.GetUnderlineColor(element.TextColor);

			TextColor = textColor;
			_activeTextinputController.InlinePlaceholderColor = placeHolderColors.InlineColor;
			_activeTextinputController.FloatingPlaceholderNormalColor = placeHolderColors.InlineColor;
			_activeTextinputController.FloatingPlaceholderActiveColor = placeHolderColors.FloatingColor;

			// BackgroundColor
			_activeTextinputController.BorderFillColor = MaterialColors.CreateEntryFilledInputBackgroundColor(element.BackgroundColor, element.TextColor);

			_activeTextinputController.ActiveColor = underlineColors.FocusedColor;
			_activeTextinputController.NormalColor = underlineColors.UnFocusedColor;
		}

		internal void UpdatePlaceholder(IMaterialEntryRenderer element)
		{
			var placeholderText = element.Placeholder ?? String.Empty;
			_activeTextinputController.PlaceholderText = placeholderText;
			ApplyTheme(element);

			var previous = _activeTextinputController.FloatingPlaceholderScale;
			if (String.IsNullOrWhiteSpace(placeholderText))
				_activeTextinputController.FloatingPlaceholderScale = 0;
			else
				_activeTextinputController.FloatingPlaceholderScale = (float)TextInputControllerBase.FloatingPlaceholderScaleDefault;

			if (previous != _activeTextinputController.FloatingPlaceholderScale && element is IVisualElementRenderer controller)
				controller.Element?.InvalidateMeasureInternal(InvalidationTrigger.VerticalOptionsChanged);
		}


		internal void UpdateTextColor(IMaterialEntryRenderer element)
		{
			var uIColor = MaterialColors.GetEntryTextColor(element.TextColor);
			_colorScheme.OnSurfaceColor = uIColor;
			_colorScheme.PrimaryColor = uIColor;
		}

		protected virtual IColorScheming CreateColorScheme()
		{
			var returnValue = MaterialColors.Light.CreateColorScheme();
			return returnValue;
		}

		protected virtual TypographyScheme CreateTypographyScheme()
		{
			return new TypographyScheme();
		}
	}


	internal class NoCaretMaterialTextField : MaterialTextField
	{
		public NoCaretMaterialTextField(IMaterialEntryRenderer element, IFontElement fontElement) : base(element, fontElement)
		{
			SpellCheckingType = UITextSpellCheckingType.No;
			AutocorrectionType = UITextAutocorrectionType.No;
			AutocapitalizationType = UITextAutocapitalizationType.None;
		}

		public override CGRect GetCaretRectForPosition(UITextPosition position)
		{
			return new CGRect();
		}
	}

	internal class ReadOnlyMaterialTextField : NoCaretMaterialTextField
	{
		readonly HashSet<string> enableActions;

		public ReadOnlyMaterialTextField(IMaterialEntryRenderer element, IFontElement fontElement) : base(element, fontElement)
		{
			string[] actions = { "copy:", "select:", "selectAll:" };
			enableActions = new HashSet<string>(actions);
		}

		public override bool CanPerform(Selector action, NSObject withSender)
			=> enableActions.Contains(action.Name);
	}
}