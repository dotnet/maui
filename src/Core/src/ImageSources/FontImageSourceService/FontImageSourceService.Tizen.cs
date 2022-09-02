#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService
	{
		public override Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFontImageSource)imageSource, cancellationToken);

		public Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IFontImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			try
			{
				//TODO : Font Image not support
				var image = new MauiImageSource();
				return FromResult(new ImageSourceServiceResult(image, () => image.Dispose()));
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to generate font image '{Glyph}'.", imageSource.Glyph);
				throw;
			}
		}

		static Task<IImageSourceServiceResult<MauiImageSource>?> FromResult(IImageSourceServiceResult<MauiImageSource>? result) =>
			Task.FromResult(result);
	}
}