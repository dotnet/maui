using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class UriImageSourceHandler : IImageSourceHandler
	{
		public async Task<Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = new CancellationToken())
		{
			var imageLoader = imagesource as UriImageSource;
			if (imageLoader?.Uri == null)
				return null;

			Stream streamImage = await imageLoader.GetStreamAsync(cancellationToken);
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