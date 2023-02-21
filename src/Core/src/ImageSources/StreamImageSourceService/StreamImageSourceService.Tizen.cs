#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public override Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
			GetImageAsync((IStreamImageSource)imageSource, cancellationToken);

		public async Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IStreamImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var stream = await imageSource.GetStreamAsync(cancellationToken);
				var image = new MauiImageSource();

				await image.LoadSource(stream);

				return new ImageSourceServiceResult(image, image.Dispose);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image stream.");
				throw;
			}
		}
	}
}