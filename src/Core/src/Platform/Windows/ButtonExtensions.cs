#nullable enable
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this MauiButton nativeButton, IText textButton)
		{
			if (textButton is IButton button)
				UpdateContent(nativeButton, button, nativeButton.GetImage());
			else
				nativeButton.Content = textButton.Text;
		}

		public static void UpdateTextColor(this MauiButton nativeButton, ITextStyle button, UI.Xaml.Media.Brush? defaultBrush = null) =>
			nativeButton.UpdateForegroundColor(button.TextColor, defaultBrush);

		public static void UpdateCharacterSpacing(this MauiButton nativeButton, ITextStyle button) =>
			nativeButton.UpdateCharacterSpacing((int)button.CharacterSpacing);

		public static void UpdateImageSource(this MauiButton nativeButton, IButton button, WImageSource? nativeImageSource)
		{
			UpdateContent(nativeButton, button, nativeImageSource);
		}

		static void UpdateContent(this MauiButton nativeButton, IButton button, object? content)
		{
			WImage? image = null;

			var text = (button as IText)?.Text;

			if (content is WImageSource nativeImageSource)
			{
				var imageSourceSize = nativeImageSource.GetImageSourceSize();
				image = nativeButton.GetImage() ?? new();
				image.Source = nativeImageSource;
				image.Width = imageSourceSize.Width;
				image.Height = imageSourceSize.Height;

				// BitmapImage is a special case that has an event when the image is loaded
				// when this happens, we want to resize the button
				if (nativeImageSource is BitmapImage bitmapImage)
				{
					bitmapImage.ImageOpened += OnImageOpened;

					void OnImageOpened(object sender, RoutedEventArgs e)
					{
						bitmapImage.ImageOpened -= OnImageOpened;

						var actualImageSourceSize = nativeImageSource.GetImageSourceSize();
						image.Width = actualImageSourceSize.Width;
						image.Height = actualImageSourceSize.Height;
					};
				}
			}
			else if (content is WImage contentImage)
			{
				image = contentImage;
			}
			// This means the users image hasn't loaded yet but we still want to setup the container for the user
			else if (button is IImageButton ib && ib.Source != null)
			{
				image = nativeButton.GetImage() ?? new();
			}

			// No text, just the image
			if (string.IsNullOrEmpty(text))
			{
				nativeButton.Content = image;
				return;
			}
			else if (image == null)
			{
				nativeButton.Content = text;
				return;
			}

			if (image != null)
			{
				image.VerticalAlignment = VerticalAlignment.Center;
				image.HorizontalAlignment = HorizontalAlignment.Center;
				image.Stretch = WStretch.Uniform;

				if (nativeButton.Content is not StackPanel)
				{
					// Both image and text, so we need to build a container for them
					var container = CreateButtonContentContainer(image, text);
					nativeButton.Content = container;
				}
			}
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
