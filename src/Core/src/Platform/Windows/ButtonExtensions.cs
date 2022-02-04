#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateStrokeColor(this Button nativeButton, IButtonStroke buttonStroke)
		{
			var brush = buttonStroke.StrokeColor?.ToPlatform();

			if (brush is null)
			{
				nativeButton.Resources.Remove("ButtonBorderBrush");
				nativeButton.Resources.Remove("ButtonBorderBrushPointerOver");
				nativeButton.Resources.Remove("ButtonBorderBrushPressed");
				nativeButton.Resources.Remove("ButtonBorderBrushDisabled");

				nativeButton.ClearValue(Button.BorderBrushProperty);
			}
			else
			{
				nativeButton.Resources["ButtonBorderBrush"] = brush;
				nativeButton.Resources["ButtonBorderBrushPointerOver"] = brush;
				nativeButton.Resources["ButtonBorderBrushPressed"] = brush;
				nativeButton.Resources["ButtonBorderBrushDisabled"] = brush;

				nativeButton.BorderBrush = brush;
			}
		}

		public static void UpdateStrokeThickness(this Button nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeThickness >= 0)
			{
				nativeButton.Resources["ButtonBorderThemeThickness"] = WinUIHelpers.CreateThickness(buttonStroke.StrokeThickness);
			}
			else
			{
				nativeButton.Resources.Remove("ButtonBorderThemeThickness");
			}
		}

		public static void UpdateCornerRadius(this Button nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.CornerRadius >= 0)
			{
				nativeButton.Resources["ControlCornerRadius"] = WinUIHelpers.CreateCornerRadius(buttonStroke.CornerRadius);
			}
			else
			{
				nativeButton.Resources.Remove("ControlCornerRadius");
			}
		}

		public static void UpdateText(this Button nativeButton, IText text)
		{
			nativeButton.UpdateText(text.Text);
		}

		public static void UpdateText(this Button nativeButton, string text) 
		{
			if (nativeButton.GetContent<TextBlock>() is TextBlock textBlock)
			{
				textBlock.Text = text;
				textBlock.Visibility = string.IsNullOrEmpty(text)
					? UI.Xaml.Visibility.Collapsed
					: UI.Xaml.Visibility.Visible;
			}
		}

		public static void UpdateBackground(this Button nativeButton, IButton button)
		{
			var brush = button.Background?.ToPlatform();

			if (brush is null)
			{
				nativeButton.Resources.Remove("ButtonBackground");
				nativeButton.Resources.Remove("ButtonBackgroundPointerOver");
				nativeButton.Resources.Remove("ButtonBackgroundPressed");
				nativeButton.Resources.Remove("ButtonBackgroundDisabled");

				nativeButton.ClearValue(Button.BackgroundProperty);
			}
			else
			{
				nativeButton.Resources["ButtonBackground"] = brush;
				nativeButton.Resources["ButtonBackgroundPointerOver"] = brush;
				nativeButton.Resources["ButtonBackgroundPressed"] = brush;
				nativeButton.Resources["ButtonBackgroundDisabled"] = brush;

				nativeButton.Background = brush;
			}
		}

		public static void UpdateTextColor(this ButtonBase nativeButton, ITextStyle button)
		{
			var brush = button.TextColor?.ToPlatform();

			if (brush is null)
			{
				// Windows.Foundation.UniversalApiContract < 5
				nativeButton.Resources.Remove("ButtonForeground");
				nativeButton.Resources.Remove("ButtonForegroundPointerOver");
				nativeButton.Resources.Remove("ButtonForegroundPressed");
				nativeButton.Resources.Remove("ButtonForegroundDisabled");

				// Windows.Foundation.UniversalApiContract >= 5
				nativeButton.ClearValue(Button.ForegroundProperty);
			}
			else
			{
				// Windows.Foundation.UniversalApiContract < 5
				nativeButton.Resources["ButtonForeground"] = brush;
				nativeButton.Resources["ButtonForegroundPointerOver"] = brush;
				nativeButton.Resources["ButtonForegroundPressed"] = brush;
				nativeButton.Resources["ButtonForegroundDisabled"] = brush;

				// Windows.Foundation.UniversalApiContract >= 5
				nativeButton.Foreground = brush;
			}
		}

		public static void UpdatePadding(this Button nativeButton, IPadding padding) =>
			nativeButton.UpdatePadding(padding, nativeButton.GetResource<UI.Xaml.Thickness>("ButtonPadding"));

		public static void UpdateCharacterSpacing(this ButtonBase nativeButton, ITextStyle button)
		{
			var characterSpacing = button.CharacterSpacing.ToEm();

			nativeButton.CharacterSpacing = characterSpacing;

			if (nativeButton.GetContent<TextBlock>() is TextBlock textBlock)
				textBlock.CharacterSpacing = characterSpacing;
		}

		public static void UpdateImageSource(this Button nativeButton, WImageSource? nativeImageSource)
		{
			if (nativeButton.GetContent<WImage>() is WImage nativeImage)
			{
				nativeImage.Source = nativeImageSource;

				if (nativeImageSource is not null)
				{
					// set the base size if we can
					{
						var imageSourceSize = nativeImageSource.GetImageSourceSize(nativeButton);
						nativeImage.Width = imageSourceSize.Width;
						nativeImage.Height = imageSourceSize.Height;
					}

					// BitmapImage is a special case that has an event when the image is loaded
					// when this happens, we want to resize the button
					if (nativeImageSource is BitmapImage bitmapImage)
					{
						bitmapImage.ImageOpened += OnImageOpened;

						void OnImageOpened(object sender, RoutedEventArgs e)
						{
							bitmapImage.ImageOpened -= OnImageOpened;

							// check if the image that just loaded is still the current image
							var actualImageSource = sender as BitmapImage;
							if (actualImageSource is not null && nativeImage.Source == actualImageSource)
							{
								// do the actual resize
								var imageSourceSize = actualImageSource.GetImageSourceSize(nativeButton);
								nativeImage.Width = imageSourceSize.Width;
								nativeImage.Height = imageSourceSize.Height;
							}
						};
					}
				}

				nativeImage.Visibility = nativeImageSource == null
					? UI.Xaml.Visibility.Collapsed
					: UI.Xaml.Visibility.Visible;
			}
		}

		public static T? GetContent<T>(this ButtonBase nativeButton)
			where T : FrameworkElement
		{
			if (nativeButton.Content is null)
				return null;

			if (nativeButton.Content is T t)
				return t;

			if (nativeButton.Content is Panel panel)
			{
				foreach (var child in panel.Children)
				{
					if (child is T c)
						return c;
				}
			}

			return null;
		}
	}
}