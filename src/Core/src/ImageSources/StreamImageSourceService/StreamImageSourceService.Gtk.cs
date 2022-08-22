#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NativeImage = Gdk.Pixbuf;


namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public override Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IStreamImageSource)imageSource, scale, cancellationToken);

		public async Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IStreamImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var stream = await imageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);

				if (stream == null)
					throw new InvalidOperationException("Unable to load image stream.");

				var image = new NativeImage(stream);

				if (image == null)
					throw new InvalidOperationException("Unable to decode image from stream.");

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image stream.");
				throw;
			}
		}
	}
}