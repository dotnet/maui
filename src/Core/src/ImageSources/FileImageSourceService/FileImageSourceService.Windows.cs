using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFileImageSource fileImageSource)
				return GetImageSourceAsync(fileImageSource, scale, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<WImageSource>?>(null);
		}

		public async Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IFileImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var filename = imageSource.File;

			try
			{
				var image = await GetLocal(filename) ?? GetAppPackage(filename);

				var result = new ImageSourceServiceResult(image);

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", filename);
				return null;
			}
		}

		BitmapImage GetAppPackage(string filename)
		{
			var imageDirectory = Configuration?.GetImageDirectory();
			if (!string.IsNullOrEmpty(imageDirectory))
			{
				var directory = Path.GetDirectoryName(filename);
				if (string.IsNullOrEmpty(directory) || !Path.GetFullPath(directory).Equals(Path.GetFullPath(imageDirectory)))
					filename = Path.Combine(imageDirectory, filename);
			}

			return new BitmapImage(new Uri("ms-appx:///" + filename));
		}

		static async Task<BitmapImage?> GetLocal(string filename)
		{
			if (Path.IsPathRooted(filename))
			{
				try
				{
					var file = await StorageFile.GetFileFromPathAsync(filename);
					using var stream = await file.OpenReadAsync();

					var image = new BitmapImage();

					await image.SetSourceAsync(stream);

					return image;
				}
				catch
				{
				}
			}

			return null;
		}
	}
}