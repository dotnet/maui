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
	public partial class FileImageSourceService : IImageSourceServiceWithDecodeOptions
	{
		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IFileImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IFileImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			FromResult(GetImage(imageSource, null));

		// Like other file images, the image is created right away: images loaded later would not update the layout.
		// (Decoding it at a smaller size costs about as much as drawing the full image, which also decodes it.)
		Task<IImageSourceServiceResult<UIImage>?> IImageSourceServiceWithDecodeOptions.GetImageAsync(IImageSource imageSource, float scale, ImageDecodeOptions options, CancellationToken cancellationToken)
		{
			// A derived service may load images differently: use its implementation (at the image's full size).
			if (GetType() != typeof(FileImageSourceService))
				return options.FromCacheOnly ? FromResult(null) : GetImageAsync(imageSource, scale, cancellationToken);

			var fileImageSource = (IFileImageSource)imageSource;

			// Only the same image (not a changed file): the image that is shown must not change.
			if (options.FromCacheOnly && ImageDecodeOptions.GetFileVersion(fileImageSource.GetPlatformImagePath(out _)) != options.ContentVersion)
				return FromResult(null);

			return FromResult(GetImage(fileImageSource, options));
		}

		ImageSourceServiceResult? GetImage(IFileImageSource imageSource, ImageDecodeOptions? options)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				using var cgImageSource = imageSource.GetPlatformImageSource(out var loadedScale);

				// If the user doesn't specify the extension, then we fall back to just letting iOS load the file
				// This means, if users try to load a animated gif and don't specify an extension it won't animate
				// We could do a search with various extensions but that's a lot of excess churn for a scenario
				// that's easily fixed by the user. Plus, the extension is required on other platforms so it's actually
				// more consistent to not load the gif on iOS without the extension.			
				var image = cgImageSource?.GetPlatformImage(loadedScale, options) ?? imageSource.GetPlatformImage();
				if (options?.WasDownsampled == true)
					options.ContentVersion = ImageDecodeOptions.GetFileVersion(imageSource.GetPlatformImagePath(out _));

				if (image is null)
					throw new InvalidOperationException("Unable to load image file.");

				return new ImageSourceServiceResult(image, () => image.Dispose());
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image file '{File}'.", imageSource.File);

				return null;
			}
		}

		static Task<IImageSourceServiceResult<UIImage>?> FromResult(IImageSourceServiceResult<UIImage>? result) =>
			Task.FromResult(result);
	}
}