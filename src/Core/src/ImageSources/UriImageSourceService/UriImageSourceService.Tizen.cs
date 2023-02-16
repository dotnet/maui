#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IImageSource imageSource, CancellationToken cancellationToken = default) =>
			GetImageAsync((IUriImageSource)imageSource, cancellationToken);

		public Task<IImageSourceServiceResult<MauiImageSource>?> GetImageAsync(IUriImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			var uri = imageSource.Uri;

			try
			{
				var image = new MauiImageSource
				{
					ResourceUrl = uri.AbsoluteUri
				};

				var result = new ImageSourceServiceResult(image, image.Dispose);
				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", uri);
				throw;
			}
		}
		static Task<IImageSourceServiceResult<MauiImageSource>?> FromResult(IImageSourceServiceResult<MauiImageSource>? result) =>
			Task.FromResult(result);
	}
}