using System;
using System.ComponentModel;

using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.iOSSpecific.Entry;
using MTextField = MaterialComponents.TextField;
using MTextInputControllerOutlined = MaterialComponents.TextInputControllerOutlined;
using MTextInputControllerFilled = MaterialComponents.TextInputControllerFilled;
using MTextInputControllerBase = MaterialComponents.TextInputControllerBase;
using MTextInputControllerUnderline = MaterialComponents.TextInputControllerUnderline;

using Xamarin.Forms;
using MaterialComponents;

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialEntryRenderer : EntryRenderer
	{
		SemanticColorScheme _colorScheme;
		TypographyScheme _typographyScheme;


		public MaterialEntryRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
			_colorScheme = (SemanticColorScheme)CreateColorScheme();
			_typographyScheme = CreateTypographyScheme();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var result =  base.SizeThatFits(size);
			if (nfloat.IsInfinity(result.Width))
				result = Control.SystemLayoutSizeFittingSize(result, (float)UILayoutPriority.FittingSizeLevel, (float)UILayoutPriority.DefaultHigh);

			return result;
		}

		IElementController ElementController => Element as IElementController;
		MTextInputControllerBase _activeTextinputController;

		public new MTextField Control { get; private set; }

		protected override UITextField CreateNativeControl()
		{
			var field = new MTextField();
			Control = field;
			field.ClearButtonMode = UITextFieldViewMode.Never;
			_activeTextinputController = new MTextInputControllerFilled(field);
			field.TextInsetsMode = TextInputTextInsetsMode.IfContent;
			ApplyTypographyScheme();
			ApplyTheme();

			return field;
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
		
		protected override void SetBackgroundColor(Color color)
		{
			ApplyTheme();
		}

		void ApplyTypographyScheme()
		{
			if (Control == null)
				return;

			_typographyScheme.Subtitle1 = Control.Font;
			TextFieldTypographyThemer.ApplyTypographyScheme(_typographyScheme, Control);
			TextFieldTypographyThemer.ApplyTypographyScheme(_typographyScheme, _activeTextinputController);
		}

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			ApplyTypographyScheme();
		}


		protected internal override void UpdateColor()
		{
			var uIColor = MaterialColors.GetEntryTextColor(Element.TextColor);

			_colorScheme.OnSurfaceColor = uIColor;
			_colorScheme.PrimaryColor = uIColor;			

			ApplyTheme();
		}


		protected virtual void ApplyTheme()
		{
			if (_activeTextinputController == null)
				return;

			FilledTextFieldColorThemer.ApplySemanticColorScheme(_colorScheme, (MTextInputControllerFilled)_activeTextinputController);

			OverrideThemeColors();
		}

		protected virtual void OverrideThemeColors()
		{
			var textColor = MaterialColors.GetEntryTextColor(Element.TextColor);
			var placeHolderColors = MaterialColors.GetPlaceHolderColor(Element.PlaceholderColor, Element.TextColor);
			var underlineColors = MaterialColors.GetUnderlineColor(Element.TextColor);

			Control.TextColor = textColor;
			_activeTextinputController.InlinePlaceholderColor = placeHolderColors.InlineColor;
			_activeTextinputController.FloatingPlaceholderNormalColor = placeHolderColors.InlineColor;
			_activeTextinputController.FloatingPlaceholderActiveColor = placeHolderColors.FloatingColor;

			// BackgroundColor
			_activeTextinputController.BorderFillColor = MaterialColors.CreateEntryFilledInputBackgroundColor(Element.BackgroundColor, Element.TextColor);

			_activeTextinputController.ActiveColor = underlineColors.FocusedColor;
			_activeTextinputController.NormalColor = underlineColors.UnFocusedColor;
		}

		protected internal override void UpdatePlaceholder()
		{
			var placeholderText = Element.Placeholder;

			if (placeholderText == null)
				return;

			_activeTextinputController.PlaceholderText = placeholderText;
			ApplyTheme();
		}
	}
}