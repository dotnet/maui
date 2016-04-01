using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WThickness = Windows.UI.Xaml.Thickness;
using WButton = Windows.UI.Xaml.Controls.Button;
using WImage = Windows.UI.Xaml.Controls.Image;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ButtonRenderer : ViewRenderer<Button, FormsButton>
	{
		bool _fontApplied;

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var button = new FormsButton();
					button.Click += OnButtonClick;
					SetNativeControl(button);
				}

				UpdateContent();

				if (Element.BackgroundColor != Color.Default)
					UpdateBackground();

				if (Element.TextColor != Color.Default)
					UpdateTextColor();

				if (Element.BorderColor != Color.Default)
					UpdateBorderColor();

				if (Element.BorderWidth != 0)
					UpdateBorderWidth();

				if (Element.BorderRadius != (int)Button.BorderRadiusProperty.DefaultValue)
					UpdateBorderRadius();

				UpdateFont();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Button.TextProperty.PropertyName || e.PropertyName == Button.ImageProperty.PropertyName)
			{
				UpdateContent();
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				UpdateBackground();
			}
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == Button.FontProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Button.BorderColorProperty.PropertyName)
			{
				UpdateBorderColor();
			}
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName)
			{
				UpdateBorderWidth();
			}
			else if (e.PropertyName == Button.BorderRadiusProperty.PropertyName)
			{
				UpdateBorderRadius();
			}
		}

		protected override void UpdateBackgroundColor()
		{
			// Button is a special case; we don't want to set the Control's background
			// because it goes outside the bounds of the Border/ContentPresenter, 
			// which is where we might change the BorderRadius to create a rounded shape.
			return;
		}

		void OnButtonClick(object sender, RoutedEventArgs e)
		{
			Button buttonView = Element;
			if (buttonView != null)
				((IButtonController)buttonView).SendClicked();
		}

		void UpdateBackground()
		{
			Control.BackgroundColor = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : (Brush)Windows.UI.Xaml.Application.Current.Resources["ButtonBackgroundThemeBrush"];
		}

		void UpdateBorderColor()
		{
			Control.BorderBrush = Element.BorderColor != Color.Default ? Element.BorderColor.ToBrush() : (Brush)Windows.UI.Xaml.Application.Current.Resources["ButtonBorderThemeBrush"];
		}

		void UpdateBorderRadius()
		{
			Control.BorderRadius = Element.BorderRadius;
		}

		void UpdateBorderWidth()
		{
			Control.BorderThickness = Element.BorderWidth == 0d ? new WThickness(3) : new WThickness(Element.BorderWidth);
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

			var image = new WImage
			{
				Source = new BitmapImage(new Uri("ms-appx:///" + elementImage.File)),
				Width = 30,
				Height = 30,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			// No text, just the image
			if (string.IsNullOrEmpty(text))
			{
				Control.Content = image;
				return;
			}

			// Both image and text, so we need to build a container for them
			var layout = Element.ContentLayout;
			var container = new StackPanel();
			var textBlock = new TextBlock
			{
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

			Control.Content = container;

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
			Control.Foreground = Element.TextColor != Color.Default ? Element.TextColor.ToBrush() : (Brush)Windows.UI.Xaml.Application.Current.Resources["DefaultTextForegroundThemeBrush"];
		}
	}
}