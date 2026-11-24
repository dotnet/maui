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
		internal static readonly string CacheDirectory = Path.Combine(FileSystem.CacheDirectory, "com.microsoft.maui", "MauiUriImages");

		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IUriImageSource)imageSource, scale, cancellationToken);

		public async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IUriImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var imageData = await DownloadAndCacheImageAsync(imageSource, cancellationToken).ConfigureAwait(false);

				UIImage image;
				try
				{
					using var cgImageSource = imageData.GetPlatformImageSource()
						?? throw new InvalidOperationException("Unable to load image file.");
					image = cgImageSource.GetPlatformImage();
				}
				catch (Exception)
				{
					// Do not keep showing (or caching) data that cannot be decoded.
					Loader.Invalidate(imageSource.Uri);
					throw;
				}

				var result = new ImageSourceServiceResult(image, () => image.Dispose());

				return result;
			}
			catch (Exception ex)
			{
				Logger?.LogWarning(ex, "Unable to load image URI '{Uri}'.", imageSource.Uri);
				throw;
			}
		}

		internal async Task<NSData> DownloadAndCacheImageAsync(IUriImageSource imageSource, CancellationToken cancellationToken)
		{
			if (!UriImageLoader.CanLoad(imageSource))
				throw new InvalidOperationException($"Unable to load image stream from image source type '{imageSource.GetType()}'.");

			var bytes = await Loader.LoadAsync(
				imageSource.Uri,
				imageSource.CachingEnabled,
				imageSource.CacheValidity,
				UriImageLoader.GetCustomFetch(imageSource),
				cancellationToken).ConfigureAwait(false);

			return NSData.FromArray(bytes);
		}

#pragma warning disable CA1822 // Mark members as static; Disabling because these methods are public and changing them to static is potentially a breaking change
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

		internal string GetCachedFileName(IUriImageSource imageSource)
		{
			var hash = Crc64.ComputeHashString(imageSource.Uri.OriginalString);
			var ext = Path.GetExtension(imageSource.Uri.AbsolutePath);
			var filename = $"{hash}{ext}";
			return filename;
		}
#pragma warning restore CA1822 // Mark members as static
	}
}
