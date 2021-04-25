using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFileImageSource fileImageSource)
				return GetImageAsync(fileImageSource, scale, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<UIImage>?>(null);
		}

		public Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IFileImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			var filename = imageSource.File;

			try
			{
				var image = File.Exists(filename)
					? UIImage.FromFile(filename)
					: UIImage.FromBundle(filename);

				if (image == null)
					return FromResult(null);

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", filename);
				return FromResult(null);
			}
		}

		static Task<IImageSourceServiceResult<UIImage>?> FromResult(IImageSourceServiceResult<UIImage>? result) =>
			Task.FromResult(result);
	}
}