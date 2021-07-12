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
		public override Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			LoadImageAsync((IStreamImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IStreamImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var stream = await imageSource.GetStreamAsync(cancellationToken);

				if (stream == null)
					throw new InvalidOperationException("Unable to load image stream.");

				var result = await image.LoadAsync(stream, cancellationToken);
				
				if (!result)
					throw new InvalidOperationException("Unable to decode image from stream.");

				return new ImageSourceServiceResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image stream.");
				throw;
			}
		}
	}
}