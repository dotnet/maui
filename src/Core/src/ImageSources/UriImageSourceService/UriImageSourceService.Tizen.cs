#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult<Image>?> GetImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			GetImageAsync((IUriImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<Image>?> GetImageAsync(IUriImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var uri = imageSource.Uri;

			try
			{
				var isLoadComplated = await image.LoadAsync(uri, cancellationToken);

				if (!isLoadComplated)
					throw new InvalidOperationException($"Unable to load image URI '{uri}'.");

				return new ImageSourceServiceResult(image);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", uri);
				throw;
			}
		}
	}
}