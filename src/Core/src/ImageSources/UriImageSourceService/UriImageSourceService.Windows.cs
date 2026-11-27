#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media.Imaging;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageSourceAsync((IUriImageSource)imageSource, scale, cancellationToken);

		public async Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IUriImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			if (!UriImageLoader.CanLoad(imageSource))
				throw new InvalidOperationException("Unable to load URI as a stream.");

			try
			{
				var bytes = await Loader.LoadAsync(
					imageSource.Uri,
					imageSource.CachingEnabled,
					imageSource.CacheValidity,
					UriImageLoader.GetCustomFetch(imageSource),
					cancellationToken);

				var image = new BitmapImage();

				try
				{
					using var stream = new MemoryStream(bytes, writable: false);
					using var ras = stream.AsRandomAccessStream();
					await image.SetSourceAsync(ras);
				}
				catch (Exception) when (!cancellationToken.IsCancellationRequested)
				{
					// The data could not be decoded: do not keep it in the cache.
					Loader.Invalidate(imageSource.Uri);
					throw;
				}

				var result = new ImageSourceServiceResult(image);

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", imageSource.Uri);
				throw;
			}
		}
	}
}