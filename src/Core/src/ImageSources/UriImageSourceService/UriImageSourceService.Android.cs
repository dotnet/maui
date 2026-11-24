#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default)
		{
			var uriImageSource = (IUriImageSource)imageSource;
			if (!uriImageSource.IsEmpty)
			{
				try
				{
					if (UriImageLoader.CanLoad(uriImageSource))
					{
						// A valid cached file is handed to Glide right away so that Glide's memory cache can show it in the same frame.
						if (uriImageSource.CachingEnabled && Loader.TryGetCachedFile(uriImageSource.Uri, uriImageSource.CacheValidity, out var cached))
							return LoadCachedDrawableAsync(uriImageSource, cached, imageView, cancellationToken);

						return LoadDownloadedDrawableAsync(uriImageSource, imageView, cancellationToken);
					}

					var callback = new ImageLoaderCallback();

					PlatformInterop.LoadImageFromUri(imageView, uriImageSource.Uri.OriginalString, uriImageSource.CachingEnabled, callback);

					return callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", uriImageSource.Uri.OriginalString);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult?>(null);
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			var uriImageSource = (IUriImageSource)imageSource;
			if (!uriImageSource.IsEmpty)
			{
				try
				{
					if (UriImageLoader.CanLoad(uriImageSource))
						return GetDownloadedDrawableAsync(uriImageSource, context, cancellationToken);

					var drawableCallback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromUri(context, uriImageSource.Uri.OriginalString, uriImageSource.CachingEnabled, drawableCallback);

					return drawableCallback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", uriImageSource.Uri.OriginalString);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}

		async Task<IImageSourceServiceResult?> LoadCachedDrawableAsync(IUriImageSource imageSource, UriImageResult cached, ImageView imageView, CancellationToken cancellationToken)
		{
			var callback = new ImageLoaderCallback();
			PlatformInterop.LoadImageFromCachedFile(imageView, cached.FilePath!, cached.Version, callback);

			var result = await callback.Result;
			if (result is not null)
				return result;

			// The cached file could not be decoded (or was removed); download it again.
			Loader.Invalidate(imageSource.Uri);
			return await LoadDownloadedDrawableAsync(imageSource, imageView, cancellationToken);
		}

		async Task<IImageSourceServiceResult?> LoadDownloadedDrawableAsync(IUriImageSource imageSource, ImageView imageView, CancellationToken cancellationToken)
		{
			try
			{
				// Resumes on the UI thread, where Glide loads must be started.
				var image = await Loader.LoadFileAsync(
					imageSource.Uri,
					imageSource.CachingEnabled,
					imageSource.CacheValidity,
					UriImageLoader.GetCustomFetch(imageSource),
					cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();

				var callback = new ImageLoaderCallback();
				if (image.FilePath is not null)
					PlatformInterop.LoadImageFromCachedFile(imageView, image.FilePath, image.Version, callback);
				else
					PlatformInterop.LoadImageFromBytes(imageView, image.Data!, callback);

				var result = await callback.Result;
				if (result is null)
					Loader.Invalidate(imageSource.Uri);

				return result;
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", imageSource.Uri.OriginalString);
				throw;
			}
		}

		async Task<IImageSourceServiceResult<Drawable>?> GetDownloadedDrawableAsync(IUriImageSource imageSource, Context context, CancellationToken cancellationToken)
		{
			try
			{
				var image = await Loader.LoadFileAsync(
					imageSource.Uri,
					imageSource.CachingEnabled,
					imageSource.CacheValidity,
					UriImageLoader.GetCustomFetch(imageSource),
					cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();

				var callback = new ImageLoaderResultCallback();
				if (image.FilePath is not null)
					PlatformInterop.LoadImageFromCachedFile(context, image.FilePath, image.Version, callback);
				else
					PlatformInterop.LoadImageFromBytes(context, image.Data!, callback);

				var result = await callback.Result;
				if (result is null)
					Loader.Invalidate(imageSource.Uri);

				return result;
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", imageSource.Uri.OriginalString);
				throw;
			}
		}
	}
}
