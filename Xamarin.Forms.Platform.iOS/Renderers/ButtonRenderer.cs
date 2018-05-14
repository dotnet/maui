using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Foundation;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public class ButtonRenderer : ViewRenderer<Button, UIButton>
	{
		UIColor _buttonTextColorDefaultDisabled;
		UIColor _buttonTextColorDefaultHighlighted;
		UIColor _buttonTextColorDefaultNormal;
		bool _useLegacyColorManagement;
		bool _titleChanged;
		SizeF _titleSize;
		UIEdgeInsets _paddingDelta = new UIEdgeInsets();

		// This looks like it should be a const under iOS Classic,
		// but that doesn't work under iOS 
		// ReSharper disable once BuiltInTypeReferenceStyle
		// Under iOS Classic Resharper wants to suggest this use the built-in type ref
		// but under iOS that suggestion won't work
		readonly nfloat _minimumButtonHeight = 44; // Apple docs
		readonly nfloat _defaultCornerRadius = 5;

		static readonly UIControlState[] s_controlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

		public override SizeF SizeThatFits(SizeF size)
		{
			var result = base.SizeThatFits(size);

			if (result.Height < _minimumButtonHeight)
			{
				result.Height = _minimumButtonHeight; 
			}

			return result;
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

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(CreateNativeControl());

					Debug.Assert(Control != null, "Control != null");

					SetControlPropertiesFromProxy();

					_useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();

					_buttonTextColorDefaultNormal = Control.TitleColor(UIControlState.Normal);
					_buttonTextColorDefaultHighlighted = Control.TitleColor(UIControlState.Highlighted);
					_buttonTextColorDefaultDisabled = Control.TitleColor(UIControlState.Disabled);

					Control.TouchUpInside += OnButtonTouchUpInside;
					Control.TouchDown += OnButtonTouchDown;
				}

				UpdateText();
				UpdateFont();
				UpdateBorder();
				UpdateImage();
				UpdateTextColor();
				UpdatePadding();
			}
		}

		protected override UIButton CreateNativeControl()
		{
			return new UIButton(UIButtonType.System);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Button.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName || e.PropertyName == Button.CornerRadiusProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
				UpdateImage();
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
				UpdatePadding();
		}
    
		protected override void SetAccessibilityLabel()
		{
			// If we have not specified an AccessibilityLabel and the AccessibiltyLabel is current bound to the Title,
			// exit this method so we don't set the AccessibilityLabel value and break the binding.
			// This may pose a problem for users who want to explicitly set the AccessibilityLabel to null, but this
			// will prevent us from inadvertently breaking UI Tests that are using Query.Marked to get the dynamic Title 
			// of the Button.

			var elemValue = (string)Element?.GetValue(AutomationProperties.NameProperty);
			if (string.IsNullOrWhiteSpace(elemValue) && Control?.AccessibilityLabel == Control?.Title(UIControlState.Normal))
				return;

			base.SetAccessibilityLabel();
		}
		
		void SetControlPropertiesFromProxy()
		{
			foreach (UIControlState uiControlState in s_controlStates)
			{
				Control.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // if new values are null, old values are preserved.
				Control.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				Control.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
			}
		}

		void OnButtonTouchUpInside(object sender, EventArgs eventArgs)
		{
			((IButtonController)Element)?.SendReleased();
			((IButtonController)Element)?.SendClicked();
		}

		void OnButtonTouchDown(object sender, EventArgs eventArgs)
		{
			((IButtonController)Element)?.SendPressed();
		}

		void UpdateBorder()
		{
			var uiButton = Control;
			var button = Element;

			if (button.BorderColor != Color.Default)
				uiButton.Layer.BorderColor = button.BorderColor.ToCGColor();

			uiButton.Layer.BorderWidth = Math.Max(0f, (float)button.BorderWidth);

			nfloat cornerRadius = _defaultCornerRadius;

			if (button.IsSet(Button.CornerRadiusProperty) && button.CornerRadius != (int)Button.CornerRadiusProperty.DefaultValue)
				cornerRadius = button.CornerRadius;

			uiButton.Layer.CornerRadius = cornerRadius;
		}

		void UpdateFont()
		{
			Control.TitleLabel.Font = Element.ToUIFont();
		}

		async void UpdateImage()
		{
			IImageSourceHandler handler;
			FileImageSource source = Element.Image;
			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				UIImage uiimage;
				try
				{
					uiimage = await handler.LoadImageAsync(source, scale: (float)UIScreen.MainScreen.Scale);
				}
				catch (OperationCanceledException)
				{
					uiimage = null;
				}
				UIButton button = Control;
				if (button != null && uiimage != null)
				{
					button.SetImage(uiimage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), UIControlState.Normal);

					button.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

					ComputeEdgeInsets(Control, Element.ContentLayout);
				}
			}
			else
			{
				Control.SetImage(null, UIControlState.Normal);
				ClearEdgeInsets(Control);
			}
			((IVisualElementController)Element).NativeSizeChanged();
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

		void UpdateTextColor()
		{
			if (Element.TextColor == Color.Default)
			{
				Control.SetTitleColor(_buttonTextColorDefaultNormal, UIControlState.Normal);
				Control.SetTitleColor(_buttonTextColorDefaultHighlighted, UIControlState.Highlighted);
				Control.SetTitleColor(_buttonTextColorDefaultDisabled, UIControlState.Disabled);
			}
			else
			{
				var color = Element.TextColor.ToUIColor();

				Control.SetTitleColor(color, UIControlState.Normal);
				Control.SetTitleColor(color, UIControlState.Highlighted);
				Control.SetTitleColor(_useLegacyColorManagement ? _buttonTextColorDefaultDisabled : color, UIControlState.Disabled);

				Control.TintColor = color;
			}
		}

		void UpdatePadding(UIButton button = null)
		{
			var uiElement = button ?? Control;
			if (uiElement == null)
				return;
			uiElement.ContentEdgeInsets = new UIEdgeInsets(
				(float)(Element.Padding.Top + _paddingDelta.Top),
				(float)(Element.Padding.Left + _paddingDelta.Left),
				(float)(Element.Padding.Bottom + _paddingDelta.Bottom),
				(float)(Element.Padding.Right + _paddingDelta.Right)
			);
		}

		void UpdateContentEdge(UIButton button, UIEdgeInsets? delta = null)
		{
			_paddingDelta = delta ?? new UIEdgeInsets ();
			UpdatePadding(button);
		}

		void ClearEdgeInsets(UIButton button)
		{
			if (button == null)
				return;

			Control.ImageEdgeInsets = new UIEdgeInsets(0, 0, 0, 0);
			Control.TitleEdgeInsets = new UIEdgeInsets(0, 0, 0, 0);
			UpdateContentEdge (Control);
		}

		void ComputeEdgeInsets(UIButton button, Button.ButtonContentLayout layout)
		{
			if (button?.ImageView?.Image == null || string.IsNullOrEmpty(button.TitleLabel?.Text))
				return;

			var position = layout.Position;
			var spacing = (nfloat)(layout.Spacing / 2);

			if (position == Button.ButtonContentLayout.ImagePosition.Left)
			{
				button.ImageEdgeInsets = new UIEdgeInsets(0, -spacing, 0, spacing);
				button.TitleEdgeInsets = new UIEdgeInsets(0, spacing, 0, -spacing);
				UpdateContentEdge (button, new UIEdgeInsets(0, 2 * spacing, 0, 2 * spacing));
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
				UpdateContentEdge (button, new UIEdgeInsets(0, 2 * spacing, 0, 2 * spacing));
				return;
			}

			var imageVertOffset = (_titleSize.Height / 2);
			var titleVertOffset = (button.ImageView.Image.Size.Height / 2);

			var edgeOffset = (float)Math.Min(imageVertOffset, titleVertOffset);

			UpdateContentEdge (button, new UIEdgeInsets (edgeOffset, 0, edgeOffset, 0));

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
