#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFileImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IFileImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return FromResult(null);

			try
			{
				using var cgImageSource = imageSource.GetPlatformImageSource(out var loadedScale);
				if (cgImageSource is null)
					throw new InvalidOperationException("Unable to load image file.");

				var image = cgImageSource.GetPlatformImage(loadedScale);

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", imageSource.File);

				throw;
			}
		}

		static Task<IImageSourceServiceResult<UIImage>?> FromResult(IImageSourceServiceResult<UIImage>? result) =>
			Task.FromResult(result);
	}
}