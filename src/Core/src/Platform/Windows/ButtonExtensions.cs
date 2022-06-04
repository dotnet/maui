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
		public static void UpdateStrokeColor(this Button platformButton, IButtonStroke buttonStroke)
		{
			var brush = buttonStroke.StrokeColor?.ToPlatform();

			if (brush is null)
			{
				platformButton.Resources.Remove("ButtonBorderBrush");
				platformButton.Resources.Remove("ButtonBorderBrushPointerOver");
				platformButton.Resources.Remove("ButtonBorderBrushPressed");
				platformButton.Resources.Remove("ButtonBorderBrushDisabled");

				platformButton.ClearValue(Button.BorderBrushProperty);
			}
			else
			{
				platformButton.Resources["ButtonBorderBrush"] = brush;
				platformButton.Resources["ButtonBorderBrushPointerOver"] = brush;
				platformButton.Resources["ButtonBorderBrushPressed"] = brush;
				platformButton.Resources["ButtonBorderBrushDisabled"] = brush;

				platformButton.BorderBrush = brush;
			}
		}

		public static void UpdateStrokeThickness(this Button platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeThickness >= 0)
			{
				platformButton.Resources["ButtonBorderThemeThickness"] = WinUIHelpers.CreateThickness(buttonStroke.StrokeThickness);
			}
			else
			{
				platformButton.Resources.Remove("ButtonBorderThemeThickness");
			}
		}

		public static void UpdateCornerRadius(this Button platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.CornerRadius >= 0)
			{
				platformButton.Resources["ControlCornerRadius"] = WinUIHelpers.CreateCornerRadius(buttonStroke.CornerRadius);
			}
			else
			{
				platformButton.Resources.Remove("ControlCornerRadius");
			}
		}

		public static void UpdateText(this Button platformButton, IText text)
		{
			platformButton.UpdateText(text.Text);
		}

		public static void UpdateText(this Button platformButton, string text)
		{
			if (platformButton.GetContent<TextBlock>() is TextBlock textBlock)
			{
				textBlock.Text = text;
				textBlock.Visibility = string.IsNullOrEmpty(text)
					? UI.Xaml.Visibility.Collapsed
					: UI.Xaml.Visibility.Visible;
			}
		}

		public static void UpdateBackground(this Button platformButton, IButton button)
		{
			var brush = button.Background?.ToPlatform();

			if (brush is null)
			{
				platformButton.Resources.Remove("ButtonBackground");
				platformButton.Resources.Remove("ButtonBackgroundPointerOver");
				platformButton.Resources.Remove("ButtonBackgroundPressed");
				platformButton.Resources.Remove("ButtonBackgroundDisabled");

				platformButton.ClearValue(Button.BackgroundProperty);
			}
			else
			{
				platformButton.Resources["ButtonBackground"] = brush;
				platformButton.Resources["ButtonBackgroundPointerOver"] = brush;
				platformButton.Resources["ButtonBackgroundPressed"] = brush;
				platformButton.Resources["ButtonBackgroundDisabled"] = brush;

				platformButton.Background = brush;
			}
		}

		public static void UpdateTextColor(this ButtonBase platformButton, ITextStyle button)
		{
			UpdateTextColor(platformButton, button.TextColor);
		}

		public static void UpdateTextColor(this ButtonBase platformButton, Color textColor)
		{
			var brush = textColor?.ToPlatform();
			if (brush is null)
			{
				// Windows.Foundation.UniversalApiContract < 5
				platformButton.Resources.Remove("ButtonForeground");
				platformButton.Resources.Remove("ButtonForegroundPointerOver");
				platformButton.Resources.Remove("ButtonForegroundPressed");
				platformButton.Resources.Remove("ButtonForegroundDisabled");

				// Windows.Foundation.UniversalApiContract >= 5
				platformButton.ClearValue(Button.ForegroundProperty);
			}
			else
			{
				// Windows.Foundation.UniversalApiContract < 5
				platformButton.Resources["ButtonForeground"] = brush;
				platformButton.Resources["ButtonForegroundPointerOver"] = brush;
				platformButton.Resources["ButtonForegroundPressed"] = brush;
				platformButton.Resources["ButtonForegroundDisabled"] = brush;

				// Windows.Foundation.UniversalApiContract >= 5
				platformButton.Foreground = brush;
			}
		}

		public static void UpdatePadding(this Button platformButton, IPadding padding) =>
			platformButton.UpdatePadding(padding, platformButton.GetResource<UI.Xaml.Thickness>("ButtonPadding"));

		public static void UpdateCharacterSpacing(this ButtonBase platformButton, ITextStyle button)
		{
			var characterSpacing = button.CharacterSpacing.ToEm();

			platformButton.CharacterSpacing = characterSpacing;

			if (platformButton.GetContent<TextBlock>() is TextBlock textBlock)
				textBlock.CharacterSpacing = characterSpacing;
		}

		public static void UpdateImageSource(this Button platformButton, WImageSource? nativeImageSource)
		{
			if (platformButton.GetContent<WImage>() is WImage nativeImage)
			{
				nativeImage.Source = nativeImageSource;

				if (nativeImageSource is not null)
				{
					// set the base size if we can
					{
						var imageSourceSize = nativeImageSource.GetImageSourceSize(platformButton);
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

							// Check if the image that just loaded is still the current image
							var actualImageSource = sender as BitmapImage;

							if (actualImageSource is not null && nativeImage.Source == actualImageSource)
								nativeImage.Height = nativeImage.Width = Primitives.Dimension.Unset;

							if (platformButton.Parent is FrameworkElement frameworkElement)
								frameworkElement.InvalidateMeasure();
						};
					}
				}

				nativeImage.Visibility = nativeImageSource == null
					? UI.Xaml.Visibility.Collapsed
					: UI.Xaml.Visibility.Visible;
			}
		}

		public static T? GetContent<T>(this ButtonBase platformButton)
			where T : FrameworkElement
		{
			if (platformButton.Content is null)
				return null;

			if (platformButton.Content is T t)
				return t;

			if (platformButton.Content is Panel panel)
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