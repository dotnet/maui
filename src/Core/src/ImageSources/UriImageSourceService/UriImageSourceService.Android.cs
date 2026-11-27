#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService : IViewSizedImageSourceService
	{
		public override Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default)
		{
			var uriImageSource = (IUriImageSource)imageSource;
			if (!uriImageSource.IsEmpty)
			{
				try
				{
					// Glide keeps resolving other schemes (content://, file://, android.resource://) as before.
					if (UriImageLoader.CanLoad(uriImageSource.Uri))
					{
						// A valid cached file is handed to Glide right away so that Glide's memory cache can show it in the same frame.
						if (uriImageSource.CachingEnabled && Loader.TryGetCachedFile(uriImageSource.Uri, uriImageSource.CacheValidity, out var cached))
							return LoadCachedDrawableAsync(uriImageSource, cached, imageView, cancellationToken);

						return LoadDownloadedDrawableAsync(uriImageSource, imageView, cancellationToken);
					}

					var callback = new ImageLoaderCallback();

					PlatformInterop.LoadImageFromUri(imageView, uriImageSource.Uri.OriginalString, uriImageSource.CachingEnabled, callback);

					return WaitForGlideAsync(callback.Result, cancellationToken);
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", uriImageSource.Uri.OriginalString);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult?>(null);
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default) =>
			GetDrawableAsync((IUriImageSource)imageSource, context, IViewSizedImageSourceService.SizeOriginal, IViewSizedImageSourceService.SizeOriginal, cancellationToken);

		Task<IImageSourceServiceResult<Drawable>?> IViewSizedImageSourceService.GetDrawableAsync(IImageSource imageSource, Context context, int width, int height, CancellationToken cancellationToken) =>
			// A derived service may load images differently: use its implementation (at the image's own size).
			GetType() == typeof(UriImageSourceService)
				? GetDrawableAsync((IUriImageSource)imageSource, context, width, height, cancellationToken)
				: GetDrawableAsync(imageSource, context, cancellationToken);

		Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IUriImageSource uriImageSource, Context context, int width, int height, CancellationToken cancellationToken)
		{
			if (!uriImageSource.IsEmpty)
			{
				try
				{
					if (UriImageLoader.CanLoad(uriImageSource.Uri))
						return GetDownloadedDrawableAsync(uriImageSource, context, width, height, cancellationToken);

					var drawableCallback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromUri(context, uriImageSource.Uri.OriginalString, uriImageSource.CachingEnabled, width, height, drawableCallback);

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

		/// <summary>
		/// Waits for a Glide load, but stops waiting when the load is cancelled (for example replaced by a newer load):
		/// Glide does not report loads that it clears before they complete.
		/// </summary>
		static Task<T?> WaitForGlideAsync<T>(Task<T?> glideLoad, CancellationToken cancellationToken)
			where T : class, IDisposable
		{
			if (!cancellationToken.CanBeCanceled || glideLoad.IsCompleted)
				return glideLoad;

			var wait = glideLoad.WaitAsync(cancellationToken);

			// Nobody receives an image that is loaded after the wait was cancelled, so release it (and Glide's resources) here.
			wait.ContinueWith(
				static (_, state) => ((Task<T?>)state!).ContinueWith(
					static load => load.Result?.Dispose(),
					CancellationToken.None,
					TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default),
				glideLoad,
				CancellationToken.None,
				TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);

			return wait;
		}

		static bool CanStillLoadInto(ImageView imageView, CancellationToken cancellationToken) =>
			!cancellationToken.IsCancellationRequested &&
			imageView.IsAlive() &&
			imageView.Context is { } context &&
			!PlatformInterop.IsContextDestroyed(context);

		async Task<IImageSourceServiceResult?> LoadCachedDrawableAsync(IUriImageSource imageSource, UriImageResult cached, ImageView imageView, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var callback = new ImageLoaderCallback();
			PlatformInterop.LoadImageFromCachedFile(imageView, cached.FilePath!, cached.Version, callback);

			var result = await WaitForGlideAsync(callback.Result, cancellationToken);
			if (result is not null || !CanStillLoadInto(imageView, cancellationToken))
				return result;

			// The cached file could not be decoded (or was removed); download it again.
			Loader.Invalidate(imageSource.Uri);
			return await LoadDownloadedDrawableAsync(imageSource, imageView, cancellationToken);
		}

		async Task<IImageSourceServiceResult?> LoadDownloadedDrawableAsync(IUriImageSource imageSource, ImageView imageView, CancellationToken cancellationToken)
		{
			try
			{
				// Like starting a Glide load, stop any previous load into this view (for example a recycled cell)
				// so that it cannot show its image while this one downloads.
				PlatformInterop.ClearImageView(imageView);

				// Resumes on the UI thread, where Glide loads must be started.
				var image = await Loader.LoadFileAsync(
					imageSource.Uri,
					imageSource.CachingEnabled,
					imageSource.CacheValidity,
					UriImageLoader.GetCustomFetch(imageSource),
					cancellationToken);

				cancellationToken.ThrowIfCancellationRequested();
				if (!imageView.IsAlive())
					return null;

				var callback = new ImageLoaderCallback();
				if (image.FilePath is not null)
					PlatformInterop.LoadImageFromCachedFile(imageView, image.FilePath, image.Version, callback);
				else
					PlatformInterop.LoadImageFromDownloadedBytes(imageView, image.Data!, callback);

				var result = await WaitForGlideAsync(callback.Result, cancellationToken);

				// Only a decoding failure means the cached data is bad (not a cancelled load or a destroyed view).
				if (result is null && CanStillLoadInto(imageView, cancellationToken))
					Loader.Invalidate(imageSource.Uri);

				return result;
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", imageSource.Uri.OriginalString);
				throw;
			}
		}

		async Task<IImageSourceServiceResult<Drawable>?> GetDownloadedDrawableAsync(IUriImageSource imageSource, Context context, int width, int height, CancellationToken cancellationToken)
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
					PlatformInterop.LoadImageFromCachedFile(context, image.FilePath, image.Version, width, height, callback);
				else
					PlatformInterop.LoadImageFromBytes(context, image.Data!, width, height, callback);

				var result = await WaitForGlideAsync(callback.Result, cancellationToken);

				// Only a decoding failure means the cached data is bad (not a cancelled load or a destroyed context).
				if (result is null && !cancellationToken.IsCancellationRequested && !PlatformInterop.IsContextDestroyed(context))
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
