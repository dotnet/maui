using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

#if IOS || MACCATALYST
using PlatformImage = UIKit.UIImage;
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformImage = System.Object;
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Platform
{
	public partial class ImageSourcePartLoader
	{
#if IOS || ANDROID || WINDOWS
		IImageSourceServiceProvider? _imageSourceServiceProvider;
#endif

#if ANDROID
		// This is a temporary workaround for Android so that images don't just keep vanishing
		// We will have a better fix in the next release that's better integrated with Glide
		internal bool CheckForImageLoadedOnAttached { get; set; }
#endif

		readonly IImageSourcePartSetter _setter;

		internal ImageSourceServiceResultManager SourceManager { get; } = new ImageSourceServiceResultManager();

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use ImageSourcePartLoader(IImageSourcePartSetter setter) instead.", true)]
		public ImageSourcePartLoader(IElementHandler handler, Func<IImageSourcePart?> imageSourcePart, Action<PlatformImage?> setImage)
			: this((IImageSourcePartSetter)handler)
		{
		}

		public ImageSourcePartLoader(IImageSourcePartSetter setter) =>
			_setter = setter;

		internal IImageSourcePartSetter Setter => _setter;

		public void Reset()
		{
			SourceManager.Reset();
#if IOS || ANDROID || WINDOWS
			_fallbackGeneration++;
			_isLoadedImageShown = false;
#if IOS
			_decodedFor = null;
#endif
			CancelFallbacks();
#if ANDROID
			// Glide may reuse the bitmap of an image that the loader owns as soon as it is released, so stop showing it first.
			if (Setter.Handler?.PlatformView is global::Android.Widget.ImageView imageView &&
				_ownedResult is IImageSourceServiceResult<PlatformImage> { Value: { } ownedImage } &&
				imageView.Drawable == ownedImage)
			{
				imageView.SetImageDrawable(null);
			}
#endif
			ReleaseOwnedResult();
#endif

			// The cancelled load does not report that it stopped loading.
			Setter.ImageSourcePart?.UpdateIsLoading(false);
		}

		public async Task UpdateImageSourceAsync()
		{
			if (Setter.Handler is not IElementHandler handler || handler.PlatformView is not PlatformView platformView)
			{
				return;
			}

			var token = SourceManager.BeginLoad();
			var imageSource = Setter.ImageSourcePart;
#if IOS || ANDROID || WINDOWS
			var fallbackGeneration = ++_fallbackGeneration;
			CancelFallbacks();
			var fallbackToken = (_fallbackCancellation = new CancellationTokenSource()).Token;
#endif

			if (imageSource?.Source is { IsEmpty: false })
			{
#if IOS || ANDROID || WINDOWS
				_imageSourceServiceProvider ??= handler.GetRequiredService<IImageSourceServiceProvider>();

				var fallbacks = imageSource as IImageSourcePartFallbacks;
				var errorSource = fallbacks?.ErrorSource;

				// Only an image that loaded successfully is kept (not a placeholder or an error image).
				var keepPrevious = _isLoadedImageShown && fallbacks?.KeepPreviousImageWhileLoading == true;
#if ANDROID
				// Otherwise Android clears the image as soon as the new one starts loading.
				_isLoadedImageShown &= keepPrevious;
#endif
				Action<Exception>? onFailed = errorSource is null
					? null
					: _ => ShowFallbackAsync(errorSource, _imageSourceServiceProvider, platformView, fallbackGeneration, () => true, fallbackToken).FireAndForget();
#endif

#if IOS
				var scale = handler.MauiContext?.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
				_decodedFor = null;
				var decodeOptions = GetDecodeOptions(platformView, scale, handler);
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, scale, token, onFailed, decodeOptions);
#elif WINDOWS
				var scale = handler.MauiContext?.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, scale, token, onFailed);
#elif ANDROID
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, token, onFailed, keepPrevious);
#endif

#if IOS || ANDROID || WINDOWS
				// Only show the placeholder when the image is not available right away (for example from a memory cache),
				// and the current image is not kept while the new one loads.
				CancellationTokenSource? placeholderCancellation = null;
				if (!loadTask.IsCompleted && !keepPrevious && fallbacks?.PlaceholderSource is { } placeholderSource)
				{
					placeholderCancellation = CancellationTokenSource.CreateLinkedTokenSource(fallbackToken);
					ShowFallbackAsync(placeholderSource, _imageSourceServiceProvider, platformView, fallbackGeneration, () => !loadTask.IsCompleted, placeholderCancellation.Token)
						.FireAndForget();
				}

				var result = await loadTask;

				// The placeholder is no longer needed (the image or the error image replaces it).
				placeholderCancellation?.Cancel();

				// A newer load has started (or the loader was reset); it owns the view and the fallbacks now.
				if (token.IsCancellationRequested)
				{
#if !ANDROID
					// Only possible when the image was replaced from an ImageLoaded handler; it is no longer shown.
					// (On Android, Glide manages the lifetime of images it set on the view.)
					result?.Dispose();
#endif
					return;
				}

				_isLoadedImageShown = result is not null;
#if IOS
				_decodedFor = result is not null && decodeOptions?.WasDownsampled == true ? decodeOptions : null;
#endif

#if ANDROID
				if (keepPrevious && result is not null)
				{
					// The loader set this image on the view itself (instead of Glide), so it releases it once it is replaced.
					SourceManager.CompleteLoad(null);
					ReplaceOwnedResult(result);
				}
				else
#endif
				{
					SourceManager.CompleteLoad(result);

					// Release the placeholder (or kept image) once the image replaced it. After a failure the
					// placeholder stays until the error image (if any) replaces it.
					if (result is not null || errorSource is null)
						ReleaseOwnedResult();
				}
#else
				await Task.CompletedTask;
#endif

#if ANDROID
				CheckForImageLoadedOnAttached = true;
#endif
			}
			else
			{
#if ANDROID
				// Also stop a Glide load into the view that is still in progress, so it cannot show its image later.
				if (platformView is global::Android.Widget.ImageView imageView)
					PlatformInterop.ClearImageView(imageView);
#endif
				Setter.SetImageSource(null);
#if IOS || ANDROID || WINDOWS
				_isLoadedImageShown = false;
#if IOS
				_decodedFor = null;
#endif
				ReleaseOwnedResult();
#endif

				// Unless clearing the image started a new load, this "load" is complete. A load that was
				// cancelled by clearing the source does not report that it stopped loading.
				if (SourceManager.Token == token)
				{
					SourceManager.CompleteLoad(null);
					imageSource?.UpdateIsLoading(false);
				}
#if ANDROID
				CheckForImageLoadedOnAttached = false;
#endif
			}
		}

#if IOS || ANDROID || WINDOWS
		// The result of the image that the loader set on the view itself (a placeholder, an error image, or on Android
		// a kept image's replacement); released once another image replaces it.
		IDisposable? _ownedResult;
		CancellationTokenSource? _fallbackCancellation;

		// Whether the image that is shown is a source that loaded successfully (not a placeholder or error image, or nothing).
		bool _isLoadedImageShown;

		// Identifies the load that a placeholder or error image belongs to. It changes whenever a new load
		// starts or the loader is reset, even after the load itself has completed.
		int _fallbackGeneration;

		/// <summary>
		/// Shows a placeholder or error image without changing the loading state.
		/// </summary>
		async Task ShowFallbackAsync(IImageSource source, IImageSourceServiceProvider provider, PlatformView platformView, int generation, Func<bool> isStillNeeded, CancellationToken token)
		{
			// The token is cancelled when a newer load starts; results that are no longer needed are also discarded below.
			bool IsCurrent() => generation == _fallbackGeneration && isStillNeeded();

			IDisposable? result = null;
			PlatformImage? image = null;

			try
			{
				var service = provider.GetRequiredImageSourceService(source);
#if IOS
				var scale = platformView.Window?.GetDisplayDensity() ?? 1.0f;
				var platformResult = await service.GetImageAsync(source, scale, token);
				result = platformResult;
				image = platformResult?.Value;
#elif WINDOWS
				var scale = platformView.XamlRoot?.RasterizationScale is double rasterizationScale ? (float)rasterizationScale : 1.0f;
				var platformResult = await service.GetImageSourceAsync(source, scale, token);
				result = platformResult;
				image = platformResult?.Value;
#elif ANDROID
				if (platformView.Context is not { } context)
					return;
				var platformResult = await service.GetDrawableAsync(source, context, token);
				result = platformResult;
				image = platformResult?.Value;
#endif
			}
			catch (Exception)
			{
				// The fallback image could not be loaded either: show nothing.
				if (IsCurrent())
				{
					Setter.SetImageSource(null);
					_isLoadedImageShown = false;
					ReleaseOwnedResult();
				}
				return;
			}

			if (!IsCurrent() || image is null)
			{
				// The fallback was never shown.
				result?.Dispose();
				return;
			}

			Setter.SetImageSource(image);
			_isLoadedImageShown = false;
#if IOS
			_decodedFor = null;
#endif
			ReplaceOwnedResult(result);
		}

		void CancelFallbacks()
		{
			var cancellation = _fallbackCancellation;
			_fallbackCancellation = null;
			cancellation?.Cancel();
		}

		void ReplaceOwnedResult(IDisposable? result)
		{
			var previous = _ownedResult;
			_ownedResult = result;
			previous?.Dispose();
		}

		void ReleaseOwnedResult() =>
			ReplaceOwnedResult(null);
#endif

#if IOS
		// The size the image that is shown was decoded for, when it was decoded at a smaller size than its full size.
		ImageDecodeOptions? _decodedFor;
		bool _isRedecoding;

		/// <summary>
		/// Gets the size the image of an <see cref="IImage"/> is displayed at, so that a large image can be decoded at that size.
		/// </summary>
		ImageDecodeOptions? GetDecodeOptions(PlatformView platformView, float density, IElementHandler handler)
		{
			if (!RuntimeFeature.IsImageDownsamplingEnabled || platformView is not MauiImageView || Setter.ImageSourcePart is not IImage image)
				return null;

			var bounds = platformView.Bounds.Size;
			if (bounds.Width > 0 && bounds.Height > 0)
				return new ImageDecodeOptions(bounds.Width * density, bounds.Height * density, image.Aspect, isViewSizeKnown: true);

			if (Primitives.Dimension.IsExplicitSet(image.Width) && Primitives.Dimension.IsExplicitSet(image.Height) && image.Width > 0 && image.Height > 0)
				return new ImageDecodeOptions(image.Width * density, image.Height * density, image.Aspect, isViewSizeKnown: true);

			// Not laid out yet: the image is rarely shown larger than the window. If it is, it is decoded again once laid out.
			var window = platformView.Window ?? handler.MauiContext?.GetOptionalPlatformWindow();
			if (window is not null && window.Bounds.Width > 0 && window.Bounds.Height > 0)
				return new ImageDecodeOptions(window.Bounds.Width * density, window.Bounds.Height * density, image.Aspect, isViewSizeKnown: false);

			return null;
		}

		/// <summary>
		/// Decodes the image that is shown again when it was decoded at a smaller size than it is now displayed at
		/// (for example the view grew, moved to a screen with a higher density, or its aspect changed). Nothing changes
		/// for the app: the image is replaced once the sharper one is ready, without loading events.
		/// </summary>
		internal void UpdateDecodedSize()
		{
			if (_decodedFor is not { } decodedFor || _isRedecoding || SourceManager.IsLoading ||
				Setter.Handler is not IElementHandler handler || handler.PlatformView is not MauiImageView platformView || platformView.Window is null)
			{
				return;
			}

			// Only sources that can be read again (from the cache): a stream may only be readable once.
			var source = Setter.ImageSourcePart?.Source;
			if (source is not (IUriImageSource or IFileImageSource))
				return;

			var density = platformView.Window.GetDisplayDensity();
			var required = GetDecodeOptions(platformView, density, handler);
			if (required is null || !required.IsViewSizeKnown || !decodedFor.IsInsufficientFor(required))
				return;

			RedecodeAsync(source, density, required).FireAndForget();
		}

		async Task RedecodeAsync(IImageSource source, float density, ImageDecodeOptions options)
		{
			var generation = _fallbackGeneration;
			var token = _fallbackCancellation?.Token ?? CancellationToken.None;

			IImageSourceServiceResult<PlatformImage>? result = null;
			_isRedecoding = true;
			try
			{
				_imageSourceServiceProvider ??= Setter.Handler?.GetRequiredService<IImageSourceServiceProvider>();
				if (_imageSourceServiceProvider?.GetRequiredImageSourceService(source) is IImageSourceServiceWithDecodeOptions service)
					result = await service.GetImageAsync(source, density, options, token);
			}
			catch (Exception)
			{
				// Keep showing the image that is shown.
			}
			finally
			{
				_isRedecoding = false;
			}

			// Only replace the image when it is still the one this was decoded for.
			if (result?.Value is not { } image || generation != _fallbackGeneration || token.IsCancellationRequested ||
				SourceManager.IsLoading || Setter.ImageSourcePart?.Source != source || _decodedFor is null)
			{
				result?.Dispose();
				return;
			}

			Setter.SetImageSource(image);
			SourceManager.ReplaceResult(result);
			_decodedFor = options.WasDownsampled ? options : null;
		}
#endif
	}
}
