#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NativeImage = Gdk.Pixbuf;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFileImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<NativeImage>?> GetImageAsync(IFileImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			var filename = imageSource.File;

			try
			{
				var image = File.Exists(filename)
					? new NativeImage(filename)
					: NativeImage.LoadFromResource(filename);

				if (image == null)
					throw new InvalidOperationException("Unable to load image file.");

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", filename);
				throw;
			}
		}

		static Task<IImageSourceServiceResult<NativeImage>?> FromResult(IImageSourceServiceResult<NativeImage>? result) =>
			Task.FromResult(result);
	}
}