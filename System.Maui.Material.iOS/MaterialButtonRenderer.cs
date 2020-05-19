using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using MaterialComponents;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using MButton = MaterialComponents.Button;

namespace Xamarin.Forms.Material.iOS
{
	public class MaterialButtonRenderer : ViewRenderer<Button, MButton>, IImageVisualElementRenderer, IButtonLayoutRenderer
	{
		bool _isDisposed;
		UIColor _defaultBorderColor;
		nfloat _defaultBorderWidth = -1;
		ButtonScheme _defaultButtonScheme;
		ButtonScheme _buttonScheme;
		ButtonLayoutManager _buttonLayoutManager;

		public MaterialButtonRenderer()
		{
			_buttonLayoutManager = new ButtonLayoutManager(this,
				preserveInitialPadding: true,
				spacingAdjustsPadding: false,
				borderAdjustsPadding: false,
				collapseHorizontalPadding: true);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (Control != null)
			{
				Control.TouchUpInside -= OnButtonTouchUpInside;
				Control.TouchDown -= OnButtonTouchDown;
				_buttonLayoutManager?.Dispose();
				_buttonLayoutManager = null;
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var measured = base.SizeThatFits(size);
			return _buttonLayoutManager?.SizeThatFits(size, measured) ?? measured;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			// recreate the scheme
			_buttonScheme?.Dispose();
			_buttonScheme = CreateButtonScheme();

			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_defaultButtonScheme = CreateButtonScheme();

					SetNativeControl(CreateNativeControl());

					Control.TouchUpInside += OnButtonTouchUpInside;
					Control.TouchDown += OnButtonTouchDown;
				}

				UpdateFont();
				UpdateCornerRadius();
				UpdateBorder();
				UpdateTextColor();
				_buttonLayoutManager?.Update();
				ApplyTheme();
			}
		}

		protected virtual ButtonScheme CreateButtonScheme()
		{
			return new ButtonScheme
			{
				ColorScheme = MaterialColors.Light.CreateColorScheme(),
				ShapeScheme = new ShapeScheme(),
				TypographyScheme = new TypographyScheme(),
			};
		}

		protected virtual void ApplyTheme()
		{
			ContainedButtonThemer.ApplyScheme(_buttonScheme, Control);

			// Colors have to be re-applied to Character spacing
			_buttonLayoutManager?.UpdateText();

			Color textColor = Element.TextColor;

			if (textColor.IsDefault)
				Control.SetTitleColor(MaterialColors.Light.DisabledColor, UIControlState.Disabled);
			else
				Control.SetTitleColor(textColor.ToUIColor(), UIControlState.Disabled);
		}

		protected override MButton CreateNativeControl() => new MButton();

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var updatedTheme = false;
			if (e.PropertyName == Button.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
				updatedTheme = true;
			}
			else if (e.PropertyName == Button.FontProperty.PropertyName)
			{
				UpdateFont();
				updatedTheme = true;
			}
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName)
			{
				UpdateBorder();
			}
			else if (e.PropertyName == Button.CornerRadiusProperty.PropertyName)
			{
				UpdateCornerRadius();
				updatedTheme = true;
			}

			if (updatedTheme)
				ApplyTheme();
		}

		protected override void SetAccessibilityLabel()
		{
			// If we have not specified an AccessibilityLabel and the AccessibilityLabel is currently bound to the Title,
			// exit this method so we don't set the AccessibilityLabel value and break the binding.
			// This may pose a problem for users who want to explicitly set the AccessibilityLabel to null, but this
			// will prevent us from inadvertently breaking UI Tests that are using Query.Marked to get the dynamic Title
			// of the Button.

			var elemValue = (string)Element?.GetValue(AutomationProperties.NameProperty);
			if (string.IsNullOrWhiteSpace(elemValue) && Control?.AccessibilityLabel == Control?.Title(UIControlState.Normal))
				return;

			base.SetAccessibilityLabel();
		}

		void OnButtonTouchUpInside(object sender, EventArgs eventArgs)
		{
			Element?.SendReleased();
			Element?.SendClicked();
		}

		void OnButtonTouchDown(object sender, EventArgs eventArgs) => Element?.SendPressed();

		protected override void SetBackgroundColor(Color color)
		{
			if (_buttonScheme?.ColorScheme is SemanticColorScheme colorScheme)
			{
				if (color.IsDefault)
				{
					colorScheme.PrimaryColor = _defaultButtonScheme.ColorScheme.PrimaryColor;
					colorScheme.OnSurfaceColor = _defaultButtonScheme.ColorScheme.OnSurfaceColor;
				}
				else
				{
					UIColor uiColor = color.ToUIColor();

					colorScheme.PrimaryColor = uiColor;
					colorScheme.OnSurfaceColor = uiColor;
				}

				if (Control != null)
					ApplyTheme();
			}
		}

		void UpdateBorder()
		{
			// NOTE: borders are not a "supported" style of the contained
			// button, thus we don't use the themer here.
			Color borderColor = Element.BorderColor;

			if (_defaultBorderColor == null)
				_defaultBorderColor = Control.GetBorderColor(UIControlState.Normal);

			if (borderColor.IsDefault)
				Control.SetBorderColor(_defaultBorderColor, UIControlState.Normal);
			else
				Control.SetBorderColor(borderColor.ToUIColor(), UIControlState.Normal);

			double borderWidth = Element.BorderWidth;

			if (_defaultBorderWidth == -1)
				_defaultBorderWidth = Control.GetBorderWidth(UIControlState.Normal);

			// TODO: The Material button does not support borders:
			//       https://github.com/xamarin/Xamarin.Forms/issues/4951
			if (borderWidth > 1)
				borderWidth = 1;

			if (borderWidth == (double)Button.BorderWidthProperty.DefaultValue)
				Control.SetBorderWidth(_defaultBorderWidth, UIControlState.Normal);
			else
				Control.SetBorderWidth((nfloat)borderWidth, UIControlState.Normal);
		}

		void UpdateCornerRadius()
		{
			int cornerRadius = Element.CornerRadius;

			if (cornerRadius == (int)Button.CornerRadiusProperty.DefaultValue)
			{
				_buttonScheme.CornerRadius = _defaultButtonScheme.CornerRadius;
			}
			else
			{
				_buttonScheme.CornerRadius = cornerRadius;
				if (_buttonScheme.ShapeScheme is ShapeScheme shapeScheme)
				{
					shapeScheme.SmallComponentShape = new ShapeCategory(ShapeCornerFamily.Rounded, cornerRadius);
					shapeScheme.MediumComponentShape = new ShapeCategory(ShapeCornerFamily.Rounded, cornerRadius);
					shapeScheme.LargeComponentShape = new ShapeCategory(ShapeCornerFamily.Rounded, cornerRadius);
				}
			}
		}

		void UpdateFont()
		{
			if (_buttonScheme.TypographyScheme is TypographyScheme typographyScheme)
			{
				if (Element.Font == (Font)Button.FontProperty.DefaultValue)
					typographyScheme.Button = _defaultButtonScheme.TypographyScheme.Button;
				else
					typographyScheme.Button = Element.ToUIFont();
			}
		}

		void UpdateTextColor()
		{
			if (_buttonScheme.ColorScheme is SemanticColorScheme colorScheme)
			{
				Color textColor = Element.TextColor;

				if (textColor.IsDefault)
					colorScheme.OnPrimaryColor = _defaultButtonScheme.ColorScheme.OnPrimaryColor;
				else
					colorScheme.OnPrimaryColor = textColor.ToUIColor();
			}

		}

		// IImageVisualElementRenderer
		bool IImageVisualElementRenderer.IsDisposed => _isDisposed;
		void IImageVisualElementRenderer.SetImage(UIImage image) => _buttonLayoutManager.SetImage(image);
		UIImageView IImageVisualElementRenderer.GetImage() => Control?.ImageView;

		// IButtonLayoutRenderer
		UIButton IButtonLayoutRenderer.Control => Control;
		IImageVisualElementRenderer IButtonLayoutRenderer.ImageVisualElementRenderer => this;
		nfloat IButtonLayoutRenderer.MinimumHeight => _buttonScheme?.MinimumHeight ?? -1;
	}
}
