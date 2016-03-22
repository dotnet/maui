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

		void OnButtonClick(object sender, RoutedEventArgs e)
		{
			Button buttonView = Element;
			if (buttonView != null)
				((IButtonController)buttonView).SendClicked();
		}

		void UpdateBackground()
		{
			Control.Background = Element.BackgroundColor != Color.Default ? Element.BackgroundColor.ToBrush() : (Brush)Windows.UI.Xaml.Application.Current.Resources["ButtonBackgroundThemeBrush"];
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
			if (Element.Image != null)
			{
				var panel = new StackPanel { Orientation = Orientation.Horizontal };

				var image = new WImage { Source = new BitmapImage(new Uri("ms-appx:///" + Element.Image.File)), Width = 30, Height = 30, Margin = new WThickness(0, 0, 20, 0) };
				panel.Children.Add(image);
				image.ImageOpened += (sender, args) => { ((IButtonController)Element).NativeSizeChanged(); };

				if (Element.Text != null)
				{
					panel.Children.Add(new TextBlock { Text = Element.Text });
				}

				Control.Content = panel;
			}
			else
			{
				Control.Content = Element.Text;
			}
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