using System;
using System.ComponentModel;
using System.Diagnostics;
using CoreGraphics;
using Foundation;
using MaterialComponents;
using UIKit;
using Xamarin.Forms;
using MButton = MaterialComponents.Button;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Button), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialButtonRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialButtonRenderer : ViewRenderer<Button, MButton>
	{
		UIColor _defaultBorderColor;
		nfloat _defaultBorderWidth = -1;

		ButtonScheme _defaultButtonScheme;
		ButtonScheme _buttonScheme;

		bool _titleChanged;
		CGSize _titleSize;
		UIEdgeInsets _paddingDelta = new UIEdgeInsets();

		public MaterialButtonRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		protected override void Dispose(bool disposing)
		{
			if (Control != null)
			{
				Control.TouchUpInside -= OnButtonTouchUpInside;
				Control.TouchDown -= OnButtonTouchDown;
			}

			base.Dispose(disposing);
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var result = base.SizeThatFits(size);

			if (result.Height < _buttonScheme.MinimumHeight)
				result.Height = _buttonScheme.MinimumHeight;

			return result;
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

				UpdateText();
				UpdateFont();
				UpdateBorder();
				UpdateImage();
				UpdateTextColor();
				UpdatePadding();

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
		}

		protected override MButton CreateNativeControl()
		{
			return new MButton();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var updatedTheme = false;
			if (e.PropertyName == Button.TextProperty.PropertyName)
			{
				UpdateText();
			}
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
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
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
			{
				UpdateImage();
			}
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
			{
				UpdatePadding();
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

		void OnButtonTouchDown(object sender, EventArgs eventArgs)
		{
			Element?.SendPressed();
		}

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
			}
		}

		void UpdateBorder()
		{
			// NOTE: borders are not a "supported" style of the contained
			// button, thus we don't use the themer here.

			// BorderColor

			Color borderColor = Element.BorderColor;

			if (_defaultBorderColor == null)
				_defaultBorderColor = Control.GetBorderColor(UIControlState.Normal);

			if (borderColor.IsDefault)
				Control.SetBorderColor(_defaultBorderColor, UIControlState.Normal);
			else
				Control.SetBorderColor(borderColor.ToUIColor(), UIControlState.Normal);

			// BorderWidth

			double borderWidth = Element.BorderWidth;

			if (_defaultBorderWidth == -1)
				_defaultBorderWidth = Control.GetBorderWidth(UIControlState.Normal);

			if (borderWidth == (double)Button.BorderWidthProperty.DefaultValue)
				Control.SetBorderWidth(_defaultBorderWidth, UIControlState.Normal);
			else
				Control.SetBorderWidth((nfloat)borderWidth, UIControlState.Normal);
		}

		void UpdateCornerRadius()
		{
			int cornerRadius = Element.CornerRadius;

			if (cornerRadius == (int)Button.CornerRadiusProperty.DefaultValue)
				_buttonScheme.CornerRadius = _defaultButtonScheme.CornerRadius;
			else
				_buttonScheme.CornerRadius = cornerRadius;
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

		void UpdateText()
		{
			var newText = Element.Text;

			if (Control.Title(UIControlState.Normal) != newText)
			{
				Control.SetTitle(Element.Text, UIControlState.Normal);
				_titleChanged = true;
			}
		}

		async void UpdateImage()
		{
			UIButton button = Control;
			if (button == null)
				return;

			var uiimage = await Element.Image.GetNativeImageAsync();
			if (uiimage != null)
			{
				button.SetImage(uiimage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), UIControlState.Normal);
				button.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

				ComputeEdgeInsets(Control, Element.ContentLayout);

				// disable tint for now
				// button.SetImageTintColor(UIColor.White, UIControlState.Normal);
			}
			else
			{
				button.SetImage(null, UIControlState.Normal);
				ClearEdgeInsets();
			}

			Element.NativeSizeChanged();
		}

		void UpdatePadding(UIButton button = null)
		{
			var uiElement = button ?? Control;
			if (uiElement == null)
				return;

			if (Element.IsSet(Button.PaddingProperty))
			{
				uiElement.ContentEdgeInsets = new UIEdgeInsets(
					(float)(Element.Padding.Top + _paddingDelta.Top),
					(float)(Element.Padding.Left + _paddingDelta.Left),
					(float)(Element.Padding.Bottom + _paddingDelta.Bottom),
					(float)(Element.Padding.Right + _paddingDelta.Right)
				);
			}
		}

		void UpdateContentEdge(UIButton button = null, UIEdgeInsets? delta = null)
		{
			var uiElement = button ?? Control;
			if (uiElement == null)
				return;

			_paddingDelta = delta ?? new UIEdgeInsets();
			UpdatePadding(uiElement);
		}

		void ClearEdgeInsets(UIButton button = null)
		{
			var uiElement = button ?? Control;
			if (uiElement == null)
				return;

			uiElement.ImageEdgeInsets = new UIEdgeInsets(0, 0, 0, 0);
			uiElement.TitleEdgeInsets = new UIEdgeInsets(0, 0, 0, 0);
			UpdateContentEdge(uiElement);
		}

		void ComputeEdgeInsets(UIButton button, Button.ButtonContentLayout layout)
		{
			if (button?.ImageView?.Image == null || string.IsNullOrEmpty(button?.TitleLabel?.Text))
				return;

			var position = layout.Position;
			var spacing = (nfloat)(layout.Spacing / 2);

			if (position == Button.ButtonContentLayout.ImagePosition.Left)
			{
				button.ImageEdgeInsets = new UIEdgeInsets(0, -spacing, 0, spacing);
				button.TitleEdgeInsets = new UIEdgeInsets(0, spacing, 0, -spacing);
				UpdateContentEdge(button, new UIEdgeInsets(0, 2 * spacing, 0, 2 * spacing));
				return;
			}

			if (_titleChanged)
			{
				var stringToMeasure = new NSString(button.TitleLabel.Text);
				UIStringAttributes attribs = new UIStringAttributes { Font = button.TitleLabel.Font };
				_titleSize = stringToMeasure.GetSizeUsingAttributes(attribs);
				_titleChanged = false;
			}

			var labelWidth = _titleSize.Width;
			var imageWidth = button.ImageView.Image.Size.Width;

			if (position == Button.ButtonContentLayout.ImagePosition.Right)
			{
				button.ImageEdgeInsets = new UIEdgeInsets(0, labelWidth + spacing, 0, -labelWidth - spacing);
				button.TitleEdgeInsets = new UIEdgeInsets(0, -imageWidth - spacing, 0, imageWidth + spacing);
				UpdateContentEdge(button, new UIEdgeInsets(0, 2 * spacing, 0, 2 * spacing));
				return;
			}

			var imageVertOffset = (_titleSize.Height / 2);
			var titleVertOffset = (button.ImageView.Image.Size.Height / 2);

			var edgeOffset = (float)Math.Min(imageVertOffset, titleVertOffset);

			UpdateContentEdge(button, new UIEdgeInsets(edgeOffset, 0, edgeOffset, 0));

			var horizontalImageOffset = labelWidth / 2;
			var horizontalTitleOffset = imageWidth / 2;

			if (position == Button.ButtonContentLayout.ImagePosition.Bottom)
			{
				imageVertOffset = -imageVertOffset;
				titleVertOffset = -titleVertOffset;
			}

			button.ImageEdgeInsets = new UIEdgeInsets(-imageVertOffset, horizontalImageOffset, imageVertOffset, -horizontalImageOffset);
			button.TitleEdgeInsets = new UIEdgeInsets(titleVertOffset, -horizontalTitleOffset, -titleVertOffset, horizontalTitleOffset);
		}
	}
}