using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Controls.Internals;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;
using WThickness = Microsoft.UI.Xaml.Thickness;
using Microsoft.Maui.Controls.Compatibility.Platform.UAP.Extensions;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ButtonRenderer : ViewRenderer<Button, FormsButton>
	{
		bool _fontApplied;
		TextBlock _textBlock = null;

		FormsButton _button;
		PointerEventHandler _pointerPressedHandler;		

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_button = new FormsButton();
					_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
					_button.Click += OnButtonClick;
					_button.AddHandler(PointerPressedEvent, _pointerPressedHandler, true);
					_button.Loaded += ButtonOnLoaded;

					SetNativeControl(_button);
				}
				else
				{
					WireUpFormsVsm();
				}

				UpdateContent();

				//TODO: We may want to revisit this strategy later. If a user wants to reset any of these to the default, the UI won't update.
				if (Element.IsSet(VisualElement.BackgroundColorProperty) && Element.BackgroundColor != (Color)VisualElement.BackgroundColorProperty.DefaultValue)
					UpdateBackgroundBrush();

				if (Element.IsSet(VisualElement.BackgroundProperty) && (Element.Background != null && !Element.Background.IsEmpty))
					UpdateBackgroundBrush();

				if (Element.IsSet(Button.TextColorProperty) && Element.TextColor != (Color)Button.TextColorProperty.DefaultValue)
					UpdateTextColor();

				if (Element.IsSet(Button.BorderColorProperty) && Element.BorderColor != (Color)Button.BorderColorProperty.DefaultValue)
					UpdateBorderColor();

				if (Element.IsSet(Button.CharacterSpacingProperty))
					UpdateCharacterSpacing();

				if (Element.IsSet(Button.BorderWidthProperty) && Element.BorderWidth != (double)Button.BorderWidthProperty.DefaultValue)
					UpdateBorderWidth();

				if (Element.IsSet(Button.CornerRadiusProperty) && Element.CornerRadius != (int)Button.CornerRadiusProperty.DefaultValue)
					UpdateBorderRadius();

				// By default Button loads width padding 8, 4, 8 ,4
				if (Element.IsSet(Button.PaddingProperty))
					UpdatePadding();

				UpdateFont();
			}
		}

		void ButtonOnLoaded(object o, RoutedEventArgs routedEventArgs)
		{
			WireUpFormsVsm();
			UpdateLineBreakMode();
		}


		void UpdateLineBreakMode()
		{
			_textBlock = Control.GetTextBlock(Control.Content);

			_textBlock?.UpdateLineBreakMode(Element.LineBreakMode);
		}

		void WireUpFormsVsm()
		{
			if (Element.UseFormsVsm())
			{
				InterceptVisualStateManager.Hook(Control.GetFirstDescendant<Microsoft.UI.Xaml.Controls.Grid>(), Control, Element);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.IsOneOf(Button.TextProperty, Button.ImageSourceProperty, Button.TextTransformProperty))
			{
				UpdateContent();
			}
			else if (e.PropertyName == Button.CharacterSpacingProperty.PropertyName)
			{
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
			{
				UpdateBackgroundBrush();
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
			else if (e.PropertyName == Button.CornerRadiusProperty.PropertyName)
			{
				UpdateBorderRadius();
			}
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
			{
				UpdatePadding();
			}
			else if (e.PropertyName == Button.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
		}

		protected override void UpdateBackgroundColor()
		{
			// Button is a special case; we don't want to set the Control's background
			// because it goes outside the bounds of the Border/ContentPresenter, 
			// which is where we might change the BorderRadius to create a rounded shape.
			return;
		}

		protected override void UpdateBackground()
		{
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

		void UpdateBackgroundBrush()
		{
			if (Brush.IsNullOrEmpty(Element.Background))
				Control.BackgroundColor = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : (WBrush)Microsoft.UI.Xaml.Application.Current.Resources["ButtonBackgroundThemeBrush"];
			else
				Control.BackgroundColor = Element.Background.ToBrush();
		}

		void UpdateBorderColor()
		{
			Control.BorderBrush = Element.BorderColor != Color.Default ? Element.BorderColor.ToBrush() : (WBrush)Microsoft.UI.Xaml.Application.Current.Resources["ButtonBorderThemeBrush"];
		}

		void UpdateBorderRadius()
		{
			Control.BorderRadius = Element.CornerRadius;
		}

		void UpdateBorderWidth()
		{
			Control.BorderThickness = Element.BorderWidth == (double)Button.BorderWidthProperty.DefaultValue ? WinUIHelpers.CreateThickness(3) : WinUIHelpers.CreateThickness(Element.BorderWidth);
		}

		void UpdateCharacterSpacing()
		{
			Control.UpdateCharacterSpacing(Element.CharacterSpacing.ToEm());
		}

		async void UpdateContent()
		{
			var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
			var elementImage = await Element.ImageSource.ToWindowsImageSourceAsync();

			// No image, just the text
			if (elementImage == null)
			{
				Control.Content = new TextBlock { Text = text };
				Element?.InvalidateMeasureNonVirtual(InvalidationTrigger.RendererReady);
				UpdateLineBreakMode();
				return;
			}

			var size = elementImage.GetImageSourceSize();
			var image = new WImage
			{
				Source = elementImage,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Stretch = WStretch.Uniform,
				Width = size.Width,
				Height = size.Height,
			};

			// BitmapImage is a special case that has an event when the image is loaded
			// when this happens, we want to resize the button
			if (elementImage is BitmapImage bmp)
			{
				bmp.ImageOpened += (sender, args) =>
				{
					var actualSize = bmp.GetImageSourceSize();
					image.Width = actualSize.Width;
					image.Height = actualSize.Height;
					Element?.InvalidateMeasureNonVirtual(InvalidationTrigger.RendererReady);
				};
			}

			// No text, just the image
			if (string.IsNullOrEmpty(text))
			{
				Control.Content = image;
				Element?.InvalidateMeasureNonVirtual(InvalidationTrigger.RendererReady);
				return;
			}

			// Both image and text, so we need to build a container for them
			Control.Content = CreateContentContainer(Element.ContentLayout, image, text);
			Element?.InvalidateMeasureNonVirtual(InvalidationTrigger.RendererReady);
			UpdateLineBreakMode();
		}

		static StackPanel CreateContentContainer(Button.ButtonContentLayout layout, WImage image, string text)
		{
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
					image.Margin = WinUIHelpers.CreateThickness(0, 0, 0, spacing);
					container.Children.Add(image);
					container.Children.Add(textBlock);
					break;
				case Button.ButtonContentLayout.ImagePosition.Bottom:
					container.Orientation = Orientation.Vertical;
					image.Margin = WinUIHelpers.CreateThickness(0, spacing, 0, 0);
					container.Children.Add(textBlock);
					container.Children.Add(image);
					break;
				case Button.ButtonContentLayout.ImagePosition.Right:
					container.Orientation = Orientation.Horizontal;
					image.Margin = WinUIHelpers.CreateThickness(spacing, 0, 0, 0);
					container.Children.Add(textBlock);
					container.Children.Add(image);
					break;
				default:
					// Defaults to image on the left
					container.Orientation = Orientation.Horizontal;
					image.Margin = WinUIHelpers.CreateThickness(0, 0, spacing, 0);
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
			Control.Foreground = Element.TextColor != Color.Default ? Element.TextColor.ToBrush() : (WBrush)Microsoft.UI.Xaml.Application.Current.Resources["DefaultTextForegroundThemeBrush"];
		}

		void UpdatePadding()
		{
			Control.Padding = WinUIHelpers.CreateThickness(
				Element.Padding.Left,
				Element.Padding.Top,
				Element.Padding.Right,
				Element.Padding.Bottom
			);
		}

		bool _isDisposed;
		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;
			if (_button != null)
			{
				_button.Click -= OnButtonClick;
				_button.RemoveHandler(PointerPressedEvent, _pointerPressedHandler);
				_button.Loaded -= ButtonOnLoaded;				

				_pointerPressedHandler = null;
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}
	}
}
