using System;
using System.ComponentModel;
using System.Windows;
using WBrush = System.Windows.Media.Brush;
using WThickness = System.Windows.Thickness;

namespace Xamarin.Forms.Platform.WPF
{
	public class RadioButtonRenderer : ViewRenderer<RadioButton, FormsRadioButton>
	{
		bool _fontApplied;
		bool _isDisposed;
		FormsRadioButton _button;

		protected override void OnElementChanged(ElementChangedEventArgs<RadioButton> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_button = new FormsRadioButton();

					_button.Click += OnButtonClick;
					_button.AddHandler(System.Windows.Controls.Button.ClickEvent, (RoutedEventHandler)OnPointerPressed, true);
					_button.Checked += OnRadioButtonCheckedOrUnchecked;
					_button.Unchecked += OnRadioButtonCheckedOrUnchecked;

					SetNativeControl(_button);
				}

				UpdateContent();

				//TODO: We may want to revisit this strategy later. If a user wants to reset any of these to the default, the UI won't update.
				if (Element.IsSet(VisualElement.BackgroundColorProperty) && Element.BackgroundColor != (Color)VisualElement.BackgroundColorProperty.DefaultValue)
					UpdateBackground();

				if (Element.IsSet(RadioButton.TextColorProperty) && Element.TextColor != (Color)RadioButton.TextColorProperty.DefaultValue)
					UpdateTextColor();

				if (Element.IsSet(RadioButton.BorderColorProperty) && Element.BorderColor != (Color)RadioButton.BorderColorProperty.DefaultValue)
					UpdateBorderColor();

				if (Element.IsSet(RadioButton.BorderWidthProperty) && Element.BorderWidth != (double)RadioButton.BorderWidthProperty.DefaultValue)
					UpdateBorderWidth();

				if (Element.IsSet(RadioButton.CornerRadiusProperty) && Element.CornerRadius != (int)RadioButton.CornerRadiusProperty.DefaultValue)
					UpdateBorderRadius();

				if (Element.IsSet(RadioButton.PaddingProperty) && Element.Padding != (Thickness)RadioButton.PaddingProperty.DefaultValue)
					UpdatePadding();

				UpdateFont();
				UpdateCheck();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == RadioButton.ContentProperty.PropertyName || e.PropertyName == Button.ImageSourceProperty.PropertyName)
			{
				UpdateContent();
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				UpdateBackground();
			}
			else if (e.PropertyName == RadioButton.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == RadioButton.FontFamilyProperty.PropertyName 
				|| e.PropertyName == RadioButton.FontSizeProperty.PropertyName
				|| e.PropertyName == RadioButton.FontAttributesProperty.PropertyName )
			{
				UpdateFont();
			}
			else if (e.PropertyName == RadioButton.BorderColorProperty.PropertyName)
			{
				UpdateBorderColor();
			}
			else if (e.PropertyName == RadioButton.BorderWidthProperty.PropertyName)
			{
				UpdateBorderWidth();
			}
			else if (e.PropertyName == RadioButton.CornerRadiusProperty.PropertyName)
			{
				UpdateBorderRadius();
			}
			else if (e.PropertyName == RadioButton.PaddingProperty.PropertyName)
			{
				UpdatePadding();
			}
			else if (e.PropertyName == RadioButton.IsCheckedProperty.PropertyName)
			{
				UpdateCheck();
			}
		}



		void OnButtonClick(object sender, RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendReleased();
			((IButtonController)Element)?.SendClicked();
		}

		void OnPointerPressed(object sender, RoutedEventArgs e)
		{
			((IButtonController)Element)?.SendPressed();
		}

		void OnRadioButtonCheckedOrUnchecked(object sender, RoutedEventArgs e)
		{
			if (Element == null || Control == null)
			{
				return;
			}

			Element.IsChecked = Control.IsChecked == true;
		}

		protected override void UpdateBackground()
		{
			Control.BackgroundColor = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : (WBrush)System.Windows.Application.Current.Resources["ButtonBackgroundThemeBrush"];
		}

		void UpdateBorderColor()
		{
			Control.BorderBrush = Element.BorderColor != Color.Default ? Element.BorderColor.ToBrush() : (WBrush)System.Windows.Application.Current.Resources["ButtonBorderThemeBrush"];
		}

		void UpdateBorderRadius()
		{
			Control.BorderRadius = Element.CornerRadius;
		}

		void UpdateBorderWidth()
		{
			Control.BorderThickness = Element.BorderWidth == (double)RadioButton.BorderWidthProperty.DefaultValue ? new WThickness(3) : new WThickness(Element.BorderWidth);
		}

		void UpdateContent()
		{
			Control.Content = Element?.ContentAsString();
		}

		void UpdateFont()
		{
			if (Control == null || Element == null)
				return;

			Font font = Font.OfSize(Element.FontFamily, Element.FontSize).WithAttributes(Element.FontAttributes);

			if (font == Font.Default && !_fontApplied)
				return;

			Font fontToApply = font == Font.Default ? Font.SystemFontOfSize(NamedSize.Medium) : font;

			Control.ApplyFont(fontToApply);
			_fontApplied = true;
		}

		void UpdateTextColor()
		{
			Control.Foreground = Element.TextColor != Color.Default ? Element.TextColor.ToBrush() : (WBrush)System.Windows.Application.Current.Resources["DefaultTextForegroundThemeBrush"];
		}

		void UpdatePadding()
		{
			Control.Padding = new WThickness(
				Element.Padding.Left,
				Element.Padding.Top,
				Element.Padding.Right,
				Element.Padding.Bottom
			);
		}

		void UpdateCheck()
		{
			if (Control == null || Element == null)
			{
				return;
			}

			Control.IsChecked = Element.IsChecked;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;
			if (_button != null)
			{
				_button.Click -= OnButtonClick;
				_button.RemoveHandler(System.Windows.Controls.Button.ClickEvent, (RoutedEventHandler)OnPointerPressed);
				_button.Checked -= OnRadioButtonCheckedOrUnchecked;
				_button.Unchecked -= OnRadioButtonCheckedOrUnchecked;
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}
	}
}
