using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource is IUriImageSource uriImageSource)
				return GetImageAsync(uriImageSource, scale, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<UIImage>?>(null);
		}

		public async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IUriImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
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
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", imageSource.Uri);
				return null;
			}
		}
	}
}