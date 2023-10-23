#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NativeImage = Gdk.Pixbuf;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IUriImageSource)imageSource, scale, cancellationToken);

		public async Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IUriImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
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
					throw new InvalidOperationException($"Unable to load image stream from URI '{imageSource.Uri}'.");

				var image = new NativeImage(stream);

				if (image == null)
					throw new InvalidOperationException($"Unable to decode image from URI '{imageSource.Uri}'.");

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", imageSource.Uri);
				throw;
			}
		}
	}
}