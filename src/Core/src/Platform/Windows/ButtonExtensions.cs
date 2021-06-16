#nullable enable
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateBackground(this MauiButton nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateBackground(button.Background, defaultBrush);

		public static void UpdateTextColor(this MauiButton nativeButton, IButton button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateForegroundColor(button.TextColor, defaultBrush);

		public static void UpdateCharacterSpacing(this MauiButton nativeButton, IButton button) =>
			nativeButton.UpdateCharacterSpacing((int)button.CharacterSpacing);

		public static void UpdatePadding(this MauiButton nativeButton, IButton button, UI.Xaml.Thickness? defaultThickness = null) =>
			nativeButton.UpdatePadding(button.Padding, defaultThickness);

		public static void UpdateFont(this MauiButton nativeButton, IButton button, IFontManager fontManager) =>
			nativeButton.UpdateFont(button.Font, fontManager);

		public static async Task UpdateContentAsync(this MauiButton nativeButton, IButton button, IImageSourceServiceProvider? provider)
		{
			var imageSource = button.ImageSource;
			var text = button.Text;

			ImageSource? nativeImageSource = null;

			// Get native image
			if (imageSource != null && provider!= null)
			{
				var service = provider.GetRequiredImageSourceService(imageSource);
				var result = await service.GetImageSourceAsync(imageSource);
				nativeImageSource = result?.Value;
			}

			// No image, just the text
			if (nativeImageSource == null)
			{
				nativeButton.Content = text;
				button?.InvalidateMeasure();
				return;
			}

			var imageSourceSize = nativeImageSource.GetImageSourceSize();

			var image = new WImage
			{
				Source = nativeImageSource,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Stretch = WStretch.Uniform,
				Width = imageSourceSize.Width,
				Height = imageSourceSize.Height
			};

			// BitmapImage is a special case that has an event when the image is loaded
			// when this happens, we want to resize the button
			if (nativeImageSource is BitmapImage bitmapImage)
			{
				bitmapImage.ImageOpened += (sender, args) =>
				{
					var actualImageSourceSize = nativeImageSource.GetImageSourceSize();

					image.Width = actualImageSourceSize.Width;
					image.Height = actualImageSourceSize.Height;

					button?.InvalidateMeasure();
				};
			}

			// No text, just the image
			if (string.IsNullOrEmpty(text))
			{
				nativeButton.Content = image;
				button?.InvalidateMeasure();
				return;
			}

			// Both image and text, so we need to build a container for them
			nativeButton.Content = CreateButtonContentContainer(image, text);
			button?.InvalidateMeasure();
		}

		internal static StackPanel CreateButtonContentContainer(WImage image, string text)
		{
			var container = new StackPanel();

			var textBlock = new TextBlock
			{
				Text = text,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			container.HorizontalAlignment = HorizontalAlignment.Center;
			container.VerticalAlignment = VerticalAlignment.Center;

			// TODO: Use ButtonContentLayout when available
			// Defaults to image on the left
			container.Orientation = Orientation.Horizontal;
			image.Margin = new WThickness(0);

			container.Children.Add(image);
			container.Children.Add(textBlock);

			return container;
		}
	}
}