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
	public partial class StreamImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IStreamImageSource)imageSource, scale, cancellationToken);

		public async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IStreamImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				using var cgImageSource =
					await imageSource.GetPlatformImageSourceAsync(cancellationToken).ConfigureAwait(false);
				if (cgImageSource is null)
					throw new InvalidOperationException("Unable to load image file.");

				var image = cgImageSource.GetPlatformImage();

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