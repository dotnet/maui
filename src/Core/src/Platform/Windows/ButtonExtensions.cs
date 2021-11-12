#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this Button nativeButton, IText text)
		{
			if (nativeButton.GetContent<TextBlock>() is TextBlock textBlock)
			{
				var actualText = text.Text;
				textBlock.Text = actualText;
				textBlock.Visibility = string.IsNullOrEmpty(actualText)
					? UI.Xaml.Visibility.Collapsed
					: UI.Xaml.Visibility.Visible;
			}
		}

		public static void UpdateBackground(this Button nativeButton, IButton button)
		{
			var brush = button.Background?.ToNative();
			if (brush is null)
			{
				nativeButton.Resources.Remove("ButtonBackground");
				nativeButton.Resources.Remove("ButtonBackgroundPointerOver");
				nativeButton.Resources.Remove("ButtonBackgroundPressed");
				nativeButton.Resources.Remove("ButtonBackgroundDisabled");
			}
			else
			{
				nativeButton.Resources["ButtonBackground"] = brush;
				nativeButton.Resources["ButtonBackgroundPointerOver"] = brush;
				nativeButton.Resources["ButtonBackgroundPressed"] = brush;
				nativeButton.Resources["ButtonBackgroundDisabled"] = brush;
			}
		}

		public static void UpdateTextColor(this Button nativeButton, ITextStyle button)
		{
			var brush = button.TextColor?.ToNative();
			if (brush is null)
			{
				nativeButton.Resources.Remove("ButtonForeground");
				nativeButton.Resources.Remove("ButtonForegroundPointerOver");
				nativeButton.Resources.Remove("ButtonForegroundPressed");
				nativeButton.Resources.Remove("ButtonForegroundDisabled");
			}
			else
			{
				nativeButton.Resources["ButtonForeground"] = brush;
				nativeButton.Resources["ButtonForegroundPointerOver"] = brush;
				nativeButton.Resources["ButtonForegroundPressed"] = brush;
				nativeButton.Resources["ButtonForegroundDisabled"] = brush;
			}
		}

		public static void UpdatePadding(this Button nativeButton, IPadding padding) =>
			nativeButton.UpdatePadding(padding, nativeButton.GetResource<UI.Xaml.Thickness>("ButtonPadding"));

		public static void UpdateCharacterSpacing(this Button nativeButton, ITextStyle button)
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

		public static T? GetContent<T>(this Button nativeButton)
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