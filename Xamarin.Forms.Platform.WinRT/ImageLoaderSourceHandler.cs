using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public sealed class ImageLoaderSourceHandler : IImageSourceHandler
	{
		public async Task<Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesoure, CancellationToken cancellationToken = new CancellationToken())
		{
			var imageLoader = imagesoure as UriImageSource;
			if (imageLoader?.Uri == null)
				return null;

			Stream streamImage = await imageLoader.GetStreamAsync(cancellationToken);
			if (streamImage == null || !streamImage.CanRead)
			{
				return null;
			}

			using(IRandomAccessStream stream = streamImage.AsRandomAccessStream())
			{
				try
				{
					var image = new BitmapImage();
					await image.SetSourceAsync(stream);
					return image;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);

					// Because this literally throws System.Exception
					// According to https://msdn.microsoft.com/library/windows/apps/jj191522
					// this can happen if the image data is bad or the app is close to its 
					// memory limit
					return null;
				}
			}
		}
	}
}