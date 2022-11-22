#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using UIKit;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		internal string CacheDirectory = Path.Combine(FileSystem.CacheDirectory, "com.microsoft.maui", "MauiUriImages");

		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IUriImageSource)imageSource, scale, cancellationToken);

		public async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IUriImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var hash = Crc64.ComputeHashString(imageSource.Uri.OriginalString);
				var pathToImageCache = CacheDirectory + hash + ".png";

				NSData? imageData;

				if (imageSource.CachingEnabled && IsImageCached(pathToImageCache))
				{
					imageData = GetCachedImage(pathToImageCache);
				}
				else
				{
					// TODO: use a real caching library with the URI
					if (imageSource is not IStreamImageSource streamImageSource)
						return null;

					using var stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);

					if (stream == null)
						throw new InvalidOperationException($"Unable to load image stream from URI '{imageSource.Uri}'.");

					imageData = NSData.FromStream(stream);

					if (imageData == null)
						throw new InvalidOperationException("Unable to load image stream data.");

					if (imageSource.CachingEnabled)
						CacheImage(imageData, pathToImageCache);
				}

				var image = UIImage.LoadFromData(imageData, scale);

				if (image == null)
					throw new InvalidOperationException($"Unable to decode image from URI '{imageSource.Uri}'.");

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", imageSource.Uri);
				throw;
			}
		}

		public void CacheImage(NSData imageData, string path)
		{
			var directory = Path.GetDirectoryName(path);

			if (string.IsNullOrEmpty(directory))
				throw new InvalidOperationException($"Unable to get directory path name '{path}'.");

			Directory.CreateDirectory(directory);

#pragma warning disable CA1416, CA1422 // https://github.com/xamarin/xamarin-macios/issues/14619
			var result = imageData.Save(path, true);
#pragma warning restore CA1416, CA1422

			if (result == false)
				throw new InvalidOperationException($"Unable to cache image at '{path}'.");
		}

		public bool IsImageCached(string path)
		{
			return File.Exists(path);
		}

		public NSData GetCachedImage(string path)
		{
			var imageData = NSData.FromFile(path);

			if (imageData == null)
				throw new InvalidOperationException($"Unable to load image stream data from '{path}'.");

			return imageData;
		}
	}

}