using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource is IStreamImageSource streamImageSource)
				return GetImageAsync(streamImageSource, scale, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<UIImage>?>(null);
		}

		public async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IStreamImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var stream = await imageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);
				if (stream == null)
					return null;

				using var data = NSData.FromStream(stream);
				var image = UIImage.LoadFromData(data, scale);

				if (image == null)
					return null;

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image stream.");
				return null;
			}
		}
	}
}