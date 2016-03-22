using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
			button.Click += HandleButtonClick;
			SetNativeControl(button);

			UpdateContent();

			if (Element.BackgroundColor != Color.Default)
				UpdateBackground();

			if (Element.TextColor != Color.Default)
				UpdateTextColor();

			if (Element.BorderColor != Color.Default)
				UpdateBorderColor();

			if (Element.BorderWidth != 0)
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
			Button buttonView = Element;
			if (buttonView != null)
				((IButtonController)buttonView).SendClicked();
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
			Control.BorderThickness = Element.BorderWidth == 0d ? new WThickness(3) : new WThickness(Element.BorderWidth);
		}

		void UpdateContent()
		{
			if (Element.Image != null)
			{
				Control.Content = new StackPanel
				{
					Orientation = Orientation.Horizontal,
					Children =
					{
						new WImage { Source = new BitmapImage(new Uri("/" + Element.Image.File, UriKind.Relative)), Width = 30, Height = 30, Margin = new WThickness(0, 0, 20, 0) },
						new TextBlock { Text = Element.Text }
					}
				};
			}
			else
				Control.Content = Element.Text;
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