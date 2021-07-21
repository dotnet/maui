using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public sealed class UriImageSourceHandler : IImageSourceHandler, IIconElementHandler
	{
		public Task<IconElement> LoadIconElementAsync(ImageSource imagesource, CancellationToken cancellationToken = default)
		{
			var imageLoader = imagesource as UriImageSource;

			if (imageLoader?.Uri == null)
				return null;

			IconElement image = new BitmapIcon { UriSource = imageLoader?.Uri };

			return Task.FromResult(image);
		}

		public Task<Microsoft.UI.Xaml.Controls.IconSource> LoadIconSourceAsync(ImageSource imagesource, CancellationToken cancellationToken = default)
		{
			var imageLoader = imagesource as UriImageSource;

			if (imageLoader?.Uri == null)
				return null;

			Microsoft.UI.Xaml.Controls.IconSource image = new Microsoft.UI.Xaml.Controls.BitmapIconSource { UriSource = imageLoader?.Uri };

			return Task.FromResult(image);
		}

		public async Task<Microsoft.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = new CancellationToken())
		{
			var imageLoader = imagesource as UriImageSource;

			if (imageLoader?.Uri == null)
				return null;

			Stream streamImage = await ((IStreamImageSource)imageLoader).GetStreamAsync(cancellationToken);

			if (streamImage == null || !streamImage.CanRead)
			{
				return null;
			}

			using (IRandomAccessStream stream = streamImage.AsRandomAccessStream())
			{
				try
				{
					var image = new BitmapImage();
					await image.SetSourceAsync(stream);
					return image;
				}
				catch (Exception ex) 
				{
					Log.Warning("Image Loading", $"{nameof(UriImageSourceHandler)} could not load {imageLoader.Uri}: {ex}");

					// According to https://msdn.microsoft.com/library/windows/apps/jj191522
					// this can happen if the image data is bad or the app is close to its 
					// memory limit
					return null;
				}
			}
		}
	}
}