using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media.Imaging;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public override Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource is IStreamImageSource streamImageSource)
				return GetImageSourceAsync(streamImageSource, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<WImageSource>?>(null);
		}

		public async Task<IImageSourceServiceResult<WImageSource>?> GetImageSourceAsync(IStreamImageSource imageSource, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				using var stream = await imageSource.GetStreamAsync(cancellationToken);
				if (stream == null)
					return null;

				var image = new BitmapImage();

				using var ras = stream.AsRandomAccessStream();
				await image.SetSourceAsync(ras);

				var result = new ImageSourceServiceResult(image);

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