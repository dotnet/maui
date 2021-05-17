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
		public override Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IImageSource imageSource, Image image, CancellationToken cancellationToken = default) =>
			LoadImageAsync((IUriImageSource)imageSource, image, cancellationToken);

		public async Task<IImageSourceServiceResult<bool>?> LoadImageAsync(IUriImageSource imageSource, Image image, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			var uri = imageSource.Uri;

			try
			{
				var result = await image.LoadAsync(uri, cancellationToken);
				
				if (!result)
					throw new InvalidOperationException($"Unable to load image URI '{uri}'.");

				return new ImageSourceServiceResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", uri);
				throw;
			}
		}
	}
}