using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

			var image = File.Exists(filename)
				? UIImage.FromFile(filename)
				: UIImage.FromBundle(filename);

			if (image == null)
				return FromResult(null);

			var result = new ImageSourceServiceResult(image, () => image.Dispose());

			return FromResult(result);
		}

		static Task<IImageSourceServiceResult<UIImage>?> FromResult(IImageSourceServiceResult<UIImage>? result) =>
			Task.FromResult(result);
	}
}