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

			// TODO: use a real caching library with the URI
			if (imageSource is not IStreamImageSource streamImageSource)
				throw new InvalidOperationException("Unable to load URI as a stream.");

			try
			{
				using var stream = await streamImageSource.GetStreamAsync(cancellationToken);

				if (stream == null)
					throw new InvalidOperationException("Unable to load image stream.");

				var image = new BitmapImage();

				using var ras = stream.AsRandomAccessStream();
				await image.SetSourceAsync(ras);

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