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

// for now using separate renderer as there's some areas of conflict (like place holder)
// plus we want the linker to be able to link this out if not used
// possibly use base class for common behavior
[assembly: ExportRenderer(typeof(Xamarin.Forms.Entry), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialEntryRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialEntryRenderer : EntryRenderer
	{
		SemanticColorScheme _colorScheme;
		TypographyScheme _typographyScheme;

		// values based on
		// https://github.com/material-components/material-components-ios/blob/develop/components/TextFields/src/ColorThemer/MDCFilledTextFieldColorThemer.m		
		const float kFilledTextFieldActiveAlpha = 0.87f;
		const float kFilledTextFieldOnSurfaceAlpha = 0.6f;
		const float kFilledTextFieldDisabledAlpha = 0.38f;
		const float kFilledTextFieldSurfaceOverlayAlpha = 0.04f;
		const float kFilledTextFieldIndicatorLineAlpha = 0.42f;
		const float kFilledTextFieldIconAlpha = 0.54f;

		// the idea of this value is that I want Active to be the exact color the user specified
		// and then all the other colors decrease according to the Material theme setup
		static float kFilledPlaceHolderOffset = 1f - kFilledTextFieldActiveAlpha;

		public MaterialEntryRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
			_colorScheme = (SemanticColorScheme)CreateColorScheme();
			_typographyScheme = CreateTypographyScheme();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
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

		protected internal override void UpdateColor()
		{
			var textColor = Element.TextColor;
			UIColor uIColor;
			if (Element.TextColor == Color.Default)
				uIColor  = MaterialColors.Light.PrimaryColor;
			else
				uIColor = textColor.ToUIColor();

			_colorScheme.OnSurfaceColor = uIColor;
			_colorScheme.PrimaryColor = uIColor;			

			ApplyTheme();
		}

		void ApplyTypographyScheme()
		{

			TextFieldTypographyThemer.ApplyTypographyScheme(_typographyScheme, Control);
			TextFieldTypographyThemer.ApplyTypographyScheme(_typographyScheme, _activeTextinputController);
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
			// Placeholder
			if(Element.PlaceholderColor != Color.Default)
			{
				var placeholderColor = Element.PlaceholderColor.ToUIColor();
				_activeTextinputController.InlinePlaceholderColor = placeholderColor.ColorWithAlpha(kFilledTextFieldOnSurfaceAlpha + kFilledPlaceHolderOffset);
				_activeTextinputController.FloatingPlaceholderNormalColor = placeholderColor.ColorWithAlpha(kFilledTextFieldOnSurfaceAlpha + kFilledPlaceHolderOffset);
				_activeTextinputController.FloatingPlaceholderActiveColor = placeholderColor.ColorWithAlpha(kFilledTextFieldActiveAlpha + kFilledPlaceHolderOffset);
			}

			// Backgroundcolor
			if(Element.BackgroundColor != Color.Default)
				_activeTextinputController.BorderFillColor = Element.BackgroundColor.ToUIColor();
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