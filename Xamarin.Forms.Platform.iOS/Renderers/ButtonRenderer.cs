using System;
using System.Drawing;
using System.Linq;
using System.ComponentModel;
#if __UNIFIED__
using UIKit;
using CoreGraphics;
#else
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
#endif
#if __UNIFIED__
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

#else
using nfloat=System.Single;
using nint=System.Int32;
using nuint=System.UInt32;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public class ButtonRenderer : ViewRenderer<Button, UIButton>
	{
		UIColor _buttonTextColorDefaultDisabled;
		UIColor _buttonTextColorDefaultHighlighted;
		UIColor _buttonTextColorDefaultNormal;

		public override SizeF SizeThatFits(SizeF size)
		{
			var result = base.SizeThatFits(size);
			result.Height = 44; // Apple docs
			//Compensate for the insets
			if (!Control.ImageView.Hidden)
				result.Width += 10;
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (Control != null)
				Control.TouchUpInside -= OnButtonTouchUpInside;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new UIButton(UIButtonType.RoundedRect));

					_buttonTextColorDefaultNormal = Control.TitleColor(UIControlState.Normal);
					_buttonTextColorDefaultHighlighted = Control.TitleColor(UIControlState.Highlighted);
					_buttonTextColorDefaultDisabled = Control.TitleColor(UIControlState.Disabled);

					Control.TouchUpInside += OnButtonTouchUpInside;
				}

				UpdateText();
				UpdateFont();
				UpdateBorder();
				UpdateImage();
				UpdateTextColor();
			}
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
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName || e.PropertyName == Button.BorderRadiusProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundVisibility();
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
				UpdateImage();
		}

		void OnButtonTouchUpInside(object sender, EventArgs eventArgs)
		{
			if (Element != null)
				((IButtonController)Element).SendClicked();
		}

		void UpdateBackgroundVisibility()
		{
			if (Forms.IsiOS7OrNewer)
				return;

			var model = Element;
			var shouldDrawImage = model.BackgroundColor == Color.Default;

			foreach (var control in Control.Subviews.Where(sv => !(sv is UILabel)))
				control.Alpha = shouldDrawImage ? 1.0f : 0.0f;
		}

		void UpdateBorder()
		{
			var uiButton = Control;
			var button = Element;

			if (button.BorderColor != Color.Default)
				uiButton.Layer.BorderColor = button.BorderColor.ToCGColor();

			uiButton.Layer.BorderWidth = (float)button.BorderWidth;
			uiButton.Layer.CornerRadius = button.BorderRadius;

			UpdateBackgroundVisibility();
		}

		void UpdateFont()
		{
			Control.TitleLabel.Font = Element.ToUIFont();
		}

		async void UpdateImage()
		{
			IImageSourceHandler handler;
			var source = Element.Image;
			if (source != null && (handler = Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
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
				var button = Control;
				if (button != null && uiimage != null)
				{
					if (Forms.IsiOS7OrNewer)
						button.SetImage(uiimage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), UIControlState.Normal);
					else
						button.SetImage(uiimage, UIControlState.Normal);
					button.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

					Control.ImageEdgeInsets = new UIEdgeInsets(0, 0, 0, 10);
					Control.TitleEdgeInsets = new UIEdgeInsets(0, 10, 0, 0);
				}
			}
			else
			{
				Control.SetImage(null, UIControlState.Normal);
				Control.ImageEdgeInsets = new UIEdgeInsets(0, 0, 0, 0);
				Control.TitleEdgeInsets = new UIEdgeInsets(0, 0, 0, 0);
			}
			((IVisualElementController)Element).NativeSizeChanged();
		}

		void UpdateText()
		{
			Control.SetTitle(Element.Text, UIControlState.Normal);
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
				Control.SetTitleColor(Element.TextColor.ToUIColor(), UIControlState.Normal);
				Control.SetTitleColor(Element.TextColor.ToUIColor(), UIControlState.Highlighted);
				Control.SetTitleColor(_buttonTextColorDefaultDisabled, UIControlState.Disabled);

				if (Forms.IsiOS7OrNewer)
					Control.TintColor = Element.TextColor.ToUIColor();
			}
		}
	}
}