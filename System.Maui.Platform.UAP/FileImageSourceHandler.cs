using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed class FileImageSourceHandler : IImageSourceHandler, IIconElementHandler
	{
		public Task<Windows.UI.Xaml.Media.ImageSource> LoadImageAsync(ImageSource imagesource, CancellationToken cancellationToken = new CancellationToken())
		{
			Windows.UI.Xaml.Media.ImageSource image = null;
			if (imagesource is FileImageSource filesource)
			{
				UpdateImageDirectory(filesource);
				string file = filesource.File;
				image = new BitmapImage(new Uri("ms-appx:///" + file));
			}

			return Task.FromResult(image);
		}

		public Task<Microsoft.UI.Xaml.Controls.IconSource> LoadIconSourceAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken))
		{
			Microsoft.UI.Xaml.Controls.IconSource image = null;

			if (imagesource is FileImageSource filesource)
			{
				UpdateImageDirectory(filesource);
				string file = filesource.File;
				image = new Microsoft.UI.Xaml.Controls.BitmapIconSource { UriSource = new Uri("ms-appx:///" + file) };
			}

			return Task.FromResult(image);
		}

		public Task<IconElement> LoadIconElementAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken))
		{
			IconElement image = null;

			if (imagesource is FileImageSource filesource)
			{
				UpdateImageDirectory(filesource);
				string file = filesource.File;
				image = new BitmapIcon { UriSource = new Uri("ms-appx:///" + file) };
			}

			return Task.FromResult(image);
		}

		void UpdateImageDirectory(FileImageSource fileSource)
		{
			var imageDirectory = Application.Current.OnThisPlatform().GetImageDirectory();

			if (!string.IsNullOrEmpty(imageDirectory))
			{
				var filePath = fileSource.File;

				var directory = Path.GetDirectoryName(filePath);

				if (string.IsNullOrEmpty(directory) || !Path.GetFullPath(directory).Equals(Path.GetFullPath(imageDirectory)))
				{
					filePath = Path.Combine(imageDirectory, filePath);
					fileSource.File = filePath;
				}
			}
		}
	}
}