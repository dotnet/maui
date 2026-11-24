#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService : IImageSourceServiceWithDecodeOptions
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IStreamImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IStreamImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync(imageSource, null, cancellationToken);

		Task<IImageSourceServiceResult<UIImage>?> IImageSourceServiceWithDecodeOptions.GetImageAsync(IImageSource imageSource, float scale, ImageDecodeOptions options, CancellationToken cancellationToken)
		{
			// A stream may only be readable once, so it is never decoded again.
			if (options.FromCacheOnly)
				return Task.FromResult<IImageSourceServiceResult<UIImage>?>(null);

			// A derived service may load images differently: use its implementation (at the image's full size).
			if (GetType() != typeof(StreamImageSourceService))
				return GetImageAsync(imageSource, scale, cancellationToken);

			// Only decode it smaller when the size it is shown at does not depend on the image.
			return GetImageAsync((IStreamImageSource)imageSource, options.IsViewSizeKnown ? options : null, cancellationToken);
		}

		async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IStreamImageSource imageSource, ImageDecodeOptions? options, CancellationToken cancellationToken)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				using var cgImageSource =
					await imageSource.GetPlatformImageSourceAsync(cancellationToken).ConfigureAwait(false);
				if (cgImageSource is null)
					throw new InvalidOperationException("Unable to load image file.");

				var image = cgImageSource.GetPlatformImage(1, options);

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