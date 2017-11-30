using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = new CancellationToken())
		{
			Windows.UI.Xaml.Media.ImageSource image = null;
			var filesource = imagesource as FileImageSource;
			if (filesource != null)
			{
				string file = filesource.File;
				image = new BitmapImage(new Uri("ms-appx:///" + file));
			}

			return Task.FromResult(image);
		}
	}
}