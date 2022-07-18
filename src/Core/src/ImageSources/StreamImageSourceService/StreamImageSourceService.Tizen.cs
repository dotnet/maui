#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public override Task<IImageSourceServiceResult<Image>?> GetImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			GetImageAsync((IStreamImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<Image>?> GetImageAsync(IStreamImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var stream = await imageSource.GetStreamAsync(cancellationToken);

				if (stream == null)
					throw new InvalidOperationException("Unable to load image stream.");

				var isLoadComplated = await image.LoadAsync(stream, cancellationToken);

				if (!isLoadComplated)
					throw new InvalidOperationException("Unable to decode image from stream.");

				return new ImageSourceServiceResult(image);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image stream.");
				throw;
			}
		}
	}
}