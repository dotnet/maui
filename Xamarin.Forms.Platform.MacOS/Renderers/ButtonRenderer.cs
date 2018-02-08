using System;
using System.ComponentModel;
using AppKit;
using Foundation;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ButtonRenderer : ViewRenderer<Button, NSButton>
	{
		protected override void Dispose(bool disposing)
		{
			if (Control != null)
				Control.Activated -= OnButtonActivated;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var btn = new NSButton();
					btn.SetButtonType(NSButtonType.MomentaryPushIn);
					SetNativeControl(btn);

					Control.Activated += OnButtonActivated;
				}

				UpdateText();
				UpdateFont();
				UpdateBorder();
				UpdateImage();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Button.TextProperty.PropertyName || e.PropertyName == Button.TextColorProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName ||
					e.PropertyName == Button.CornerRadiusProperty.PropertyName ||
					e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundVisibility();
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
				UpdateImage();
		}

		void OnButtonActivated(object sender, EventArgs eventArgs)
		{
			((IButtonController)Element)?.SendClicked();
		}

		void UpdateBackgroundVisibility()
		{
			var model = Element;
			var shouldDrawImage = model.BackgroundColor == Color.Default;
			if (!shouldDrawImage)
				Control.Cell.BackgroundColor = model.BackgroundColor.ToNSColor();
		}

		void UpdateBorder()
		{
			var uiButton = Control;
			var button = Element;

			if (button.BorderColor != Color.Default)
				uiButton.Layer.BorderColor = button.BorderColor.ToCGColor();

			uiButton.Layer.BorderWidth = (float)button.BorderWidth;
			uiButton.Layer.CornerRadius = button.CornerRadius;

			UpdateBackgroundVisibility();
		}

		void UpdateFont()
		{
			Control.Font = Element.Font.ToNSFont();
		}

		async void UpdateImage()
		{
			IImageSourceHandler handler;
			FileImageSource source = Element.Image;
			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				NSImage uiimage;
				try
				{
					uiimage = await handler.LoadImageAsync(source);
				}
				catch (OperationCanceledException)
				{
					uiimage = null;
				}
				NSButton button = Control;
				if (button != null && uiimage != null)
				{
					button.Image = uiimage;
					if (!string.IsNullOrEmpty(button.Title))
						button.ImagePosition = Element.ToNSCellImagePosition();
				}
			}
			((IVisualElementController)Element).NativeSizeChanged();
		}

		void UpdateText()
		{
			var color = Element.TextColor;
			if (color == Color.Default)
			{
				Control.Title = Element.Text ?? "";
			}
			else
			{
				var textWithColor = new NSAttributedString(Element.Text ?? "", font: Element.Font.ToNSFont(), foregroundColor: color.ToNSColor( ), paragraphStyle: new NSMutableParagraphStyle( ) { Alignment = NSTextAlignment.Center });
				Control.AttributedTitle = textWithColor;
			}
		}
	}
}