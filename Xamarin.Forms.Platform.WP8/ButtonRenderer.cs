using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xamarin.Forms.Internals;
using WButton = System.Windows.Controls.Button;
using WImage = System.Windows.Controls.Image;
using WThickness = System.Windows.Thickness;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ButtonRenderer : ViewRenderer<Button, WButton>
	{
		bool _fontApplied;

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			var button = new WButton();
			button.ClickMode = ClickMode.Press;
			button.Click += HandleButtonClick;
			button.AddHandler(UIElement.TapEvent, new EventHandler<GestureEventArgs>(HandleButtonTap), true);
			SetNativeControl(button);

			UpdateContent();

			if (Element.BackgroundColor != Color.Default)
				UpdateBackground();

			if (Element.TextColor != Color.Default)
				UpdateTextColor();

			if (Element.BorderColor != Color.Default)
				UpdateBorderColor();

			if (Element.BorderWidth != (double)Button.BorderWidthProperty.DefaultValue)
				UpdateBorderWidth();

			UpdateFont();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Button.TextProperty.PropertyName || e.PropertyName == Button.ImageProperty.PropertyName)
				UpdateContent();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorderColor();
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName)
				UpdateBorderWidth();
		}

		void HandleButtonClick(object sender, RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendPressed();
		}

		void HandleButtonTap(object sender, GestureEventArgs e)
		{
			((IButtonController)Element)?.SendReleased();
			((IButtonController)Element)?.SendClicked();
		}

		void UpdateBackground()
		{
			Control.Background = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : (Brush)System.Windows.Application.Current.Resources["PhoneBackgroundBrush"];
		}

		void UpdateBorderColor()
		{
			Control.BorderBrush = Element.BorderColor != Color.Default ? Element.BorderColor.ToBrush() : (Brush)System.Windows.Application.Current.Resources["PhoneForegroundBrush"];
		}

		void UpdateBorderWidth()
		{
			Control.BorderThickness = Element.BorderWidth == (double)Button.BorderWidthProperty.DefaultValue ? new WThickness(3) : new WThickness(Element.BorderWidth);
		}

		void UpdateContent()
		{
			var text = Element.Text;
			var elementImage = Element.Image;

			// No image, just the text
			if (elementImage == null)
			{
				Control.Content = text;
				return;
			}

			var bmp = new BitmapImage(new Uri("/" + elementImage.File, UriKind.Relative));

			var image = new WImage
			{
				Source = bmp,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			bmp.ImageOpened += (sender, args) => {
				image.Width = bmp.PixelWidth;
				image.Height = bmp.PixelHeight;
				Element.InvalidateMeasureNonVirtual(InvalidationTrigger.RendererReady);
			};

			// No text, just the image
			if (string.IsNullOrEmpty(text))
			{
				Control.Content = image;
				return;
			}

			// Both image and text, so we need to build a container for them
			Control.Content = CreateContentContainer(Element.ContentLayout, image, text);
		}

		static StackPanel CreateContentContainer(Button.ButtonContentLayout layout, WImage image, string text)
		{
			var container = new StackPanel();
			var textBlock = new TextBlock {
				Text = text,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			var spacing = layout.Spacing;

			container.HorizontalAlignment = HorizontalAlignment.Center;
			container.VerticalAlignment = VerticalAlignment.Center;

			switch (layout.Position)
			{
				case Button.ButtonContentLayout.ImagePosition.Top:
					container.Orientation = Orientation.Vertical;
					image.Margin = new WThickness(0, 0, 0, spacing);
					container.Children.Add(image);
					container.Children.Add(textBlock);
					break;
				case Button.ButtonContentLayout.ImagePosition.Bottom:
					container.Orientation = Orientation.Vertical;
					image.Margin = new WThickness(0, spacing, 0, 0);
					container.Children.Add(textBlock);
					container.Children.Add(image);
					break;
				case Button.ButtonContentLayout.ImagePosition.Right:
					container.Orientation = Orientation.Horizontal;
					image.Margin = new WThickness(spacing, 0, 0, 0);
					container.Children.Add(textBlock);
					container.Children.Add(image);
					break;
				default:
					// Defaults to image on the left
					container.Orientation = Orientation.Horizontal;
					image.Margin = new WThickness(0, 0, spacing, 0);
					container.Children.Add(image);
					container.Children.Add(textBlock);
					break;
			}

			return container;
		}

		void UpdateFont()
		{
			if (Control == null || Element == null)
				return;

			if (Element.Font == Font.Default && !_fontApplied)
				return;

			Font fontToApply = Element.Font == Font.Default ? Font.SystemFontOfSize(NamedSize.Medium) : Element.Font;

			Control.ApplyFont(fontToApply);
			_fontApplied = true;
		}

		void UpdateTextColor()
		{
			Control.Foreground = Element.TextColor != Color.Default ? Element.TextColor.ToBrush() : (Brush)System.Windows.Application.Current.Resources["PhoneForegroundBrush"];
		}
	}
}