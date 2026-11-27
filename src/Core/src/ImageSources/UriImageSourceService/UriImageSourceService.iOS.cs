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
	public partial class UriImageSourceService : IImageSourceServiceWithDecodeOptions
	{
		internal static readonly string CacheDirectory = Path.Combine(FileSystem.CacheDirectory, "com.microsoft.maui", "MauiUriImages");

		public override Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((IUriImageSource)imageSource, scale, cancellationToken);

		public Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IUriImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync(imageSource, null, cancellationToken);

		Task<IImageSourceServiceResult<UIImage>?> IImageSourceServiceWithDecodeOptions.GetImageAsync(IImageSource imageSource, float scale, ImageDecodeOptions options, CancellationToken cancellationToken)
		{
			// A derived service may load images differently: use its implementation (at the image's full size).
			if (GetType() != typeof(UriImageSourceService))
				return options.FromCacheOnly ? Task.FromResult<IImageSourceServiceResult<UIImage>?>(null) : GetImageAsync(imageSource, scale, cancellationToken);

			var uriImageSource = (IUriImageSource)imageSource;
			if (options.FromCacheOnly)
				return GetCachedImageAsync(uriImageSource, options, cancellationToken);

			// Without the cache, the image cannot be decoded again (larger) without downloading it again, which may return
			// another image. So only decode it smaller when the size it is shown at does not depend on the image.
			return GetImageAsync(uriImageSource, uriImageSource.CachingEnabled || options.IsViewSizeKnown ? options : null, cancellationToken);
		}

		/// <summary>
		/// Decodes the image again from the cache, without downloading it, or returns <see langword="null"/> when it is not cached.
		/// </summary>
		async Task<IImageSourceServiceResult<UIImage>?> GetCachedImageAsync(IUriImageSource imageSource, ImageDecodeOptions options, CancellationToken cancellationToken)
		{
			if (imageSource.IsEmpty || !imageSource.CachingEnabled || !UriImageLoader.CanLoad(imageSource))
				return null;

			var bytes = await Loader.TryLoadCachedAsync(imageSource.Uri, cancellationToken).ConfigureAwait(false);

			// Only the same image (not a newer version of it): the image that is shown must not change.
			if (bytes is null || ImageDecodeOptions.GetContentVersion(bytes) != options.ContentVersion)
				return null;

			using var cgImageSource = NSData.FromArray(bytes).GetPlatformImageSource();
			if (cgImageSource is null)
				return null;

			var image = cgImageSource.GetPlatformImage(1, options);
			return new ImageSourceServiceResult(image, () => image.Dispose());
		}

		async Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IUriImageSource imageSource, ImageDecodeOptions? options, CancellationToken cancellationToken)
		{
			if (imageSource.IsEmpty)
				return null;

			try
			{
				var bytes = await LoadBytesAsync(imageSource, cancellationToken).ConfigureAwait(false);

				UIImage image;
				try
				{
					using var imageData = NSData.FromArray(bytes);
					using var cgImageSource = imageData.GetPlatformImageSource()
						?? throw new InvalidOperationException("Unable to load image file.");
					image = cgImageSource.GetPlatformImage(1, options);

					// Only cached images are decoded again, from the same data.
					if (options?.WasDownsampled == true && imageSource.CachingEnabled)
						options.ContentVersion = ImageDecodeOptions.GetContentVersion(bytes);
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

		internal async Task<NSData> DownloadAndCacheImageAsync(IUriImageSource imageSource, CancellationToken cancellationToken) =>
			NSData.FromArray(await LoadBytesAsync(imageSource, cancellationToken).ConfigureAwait(false));

		Task<byte[]> LoadBytesAsync(IUriImageSource imageSource, CancellationToken cancellationToken)
		{
			if (!UriImageLoader.CanLoad(imageSource))
				throw new InvalidOperationException($"Unable to load image stream from image source type '{imageSource.GetType()}'.");

			return Loader.LoadAsync(
				imageSource.Uri,
				imageSource.CachingEnabled,
				imageSource.CacheValidity,
				UriImageLoader.GetCustomFetch(imageSource),
				cancellationToken);
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
