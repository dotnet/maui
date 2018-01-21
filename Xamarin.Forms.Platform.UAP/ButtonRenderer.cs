using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms.Internals;
using WThickness = Windows.UI.Xaml.Thickness;
using WButton = Windows.UI.Xaml.Controls.Button;
using WImage = Windows.UI.Xaml.Controls.Image;
using Windows.UI.Xaml.Input;

namespace Xamarin.Forms.Platform.UWP
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
					button.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true);
					button.Loaded += ButtonOnLoaded;

					SetNativeControl(button);
				}
				else
				{
					WireUpFormsVsm();
				}

				UpdateContent();

				if (Element.BackgroundColor != Color.Default)
					UpdateBackground();

				if (Element.TextColor != Color.Default)
					UpdateTextColor();

				if (Element.BorderColor != Color.Default)
					UpdateBorderColor();

				if (Element.BorderWidth != (double)Button.BorderWidthProperty.DefaultValue)
					UpdateBorderWidth();

				if (Element.BorderRadius != (int)Button.BorderRadiusProperty.DefaultValue)
					UpdateBorderRadius();

				UpdateFont();
			}
		}

		void ButtonOnLoaded(object o, RoutedEventArgs routedEventArgs)
		{
			WireUpFormsVsm();
		}

		void WireUpFormsVsm()
		{
			if (Element.UseFormsVsm())
			{
				InterceptVisualStateManager.Hook(Control.GetFirstDescendant<Windows.UI.Xaml.Controls.Grid>(), Control, Element);
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

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnButtonClick(object sender, RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendReleased();
			((IButtonController)Element)?.SendClicked();
		}

		void OnPointerPressed(object sender, RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendPressed();
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

			var bmp = new BitmapImage(new Uri("ms-appx:///" + elementImage.File));

			var image = new WImage
			{
				Source = bmp,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Stretch = Stretch.Uniform
			};

			bmp.ImageOpened += (sender, args) => {
				image.Width = bmp.PixelWidth;
				image.Height = bmp.PixelHeight;
				Element?.InvalidateMeasureNonVirtual(InvalidationTrigger.RendererReady);
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
			Control.Foreground = Element.TextColor != Color.Default ? Element.TextColor.ToBrush() : (Brush)Windows.UI.Xaml.Application.Current.Resources["DefaultTextForegroundThemeBrush"];
		}
	}
}
