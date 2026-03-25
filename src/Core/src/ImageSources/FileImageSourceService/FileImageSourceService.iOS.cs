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

				// If the user doesn't specify the extension, then we fall back to just letting iOS load the file
				// This means, if users try to load a animated gif and don't specify an extension it won't animate
				// We could do a search with various extensions but that's a lot of excess churn for a scenario
				// that's easily fixed by the user. Plus, the extension is required on other platforms so it's actually
				// more consistent to not load the gif on iOS without the extension.			
				var image = cgImageSource?.GetPlatformImage(loadedScale) ?? imageSource.GetPlatformImage();

				if (image is null)
					throw new InvalidOperationException("Unable to load image file.");

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return FromResult(result);
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", imageSource.File);

				return FromResult(null);
			}
		}

		static Task<IImageSourceServiceResult<UIImage>?> FromResult(IImageSourceServiceResult<UIImage>? result) =>
			Task.FromResult(result);
	}
}