using System.Threading;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource is IUriImageSource fileImageSource)
				return GetImageAsync(fileImageSource, scale, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<UIImage>?>(null);
		}

		public async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IUriImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			// TODO: use a real caching library with the URI
			if (imageSource is not IStreamImageSource streamImageSource)
				return null;

			using var stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);
			if (stream == null)
				return null;

			var image = UIImage.LoadFromData(NSData.FromStream(stream), scale);
			if (image == null)
				return null;

			var result = new ImageSourceServiceResult(image, () => image.Dispose());

			return result;
		}
	}
}