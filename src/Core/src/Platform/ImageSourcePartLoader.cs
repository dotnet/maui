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
			// Reset runs when the handler is disconnected, after its PlatformView was cleared, so the view is tracked separately.
			RemoveOwnedImageFromView();
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

			var imageSource = Setter.ImageSourcePart;
#if IOS || ANDROID || WINDOWS
			// Only an image that loaded successfully is kept (not a placeholder or an error image).
			var keepPrevious = _isLoadedImageShown && imageSource?.Source is { IsEmpty: false } &&
				imageSource is IImageSourcePartFallbacks { KeepPreviousImageWhileLoading: true } &&
				CanKeepPreviousImage(handler, imageSource.Source);

			// Starting a load releases the result of the image that is shown; keep it until it is no longer shown.
			if (keepPrevious && SourceManager.DetachResult() is { } shownResult && handler.PlatformView is PlatformView shownView)
				OwnShownResult(shownResult, shownView);
#endif

			var token = SourceManager.BeginLoad();
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

#if ANDROID
				// An earlier load into the view that has not finished (for example paused by Glide) must not show its image
				// over this load's placeholder or error image. Some image sources do asynchronous work (such as reading a
				// stream) before they start their own load into the view, which would stop it. (Kept images are not loaded
				// into the view.)
				if (!keepPrevious && platformView is global::Android.Widget.ImageView unfinishedLoadView)
					PlatformInterop.StopUnfinishedImageViewLoad(unfinishedLoadView);

				// Otherwise Android clears the image as soon as the new one starts loading. Glide only does that for images
				// it set itself, not for a kept image that the loader set.
				if (!keepPrevious && _isLoadedImageShown)
				{
					RemoveOwnedImageFromView();
					ReleaseOwnedResult();
				}
				_isLoadedImageShown &= keepPrevious;

				// A kept image's replacement is set on the view by the loader itself, so the loader owns it from then on.
				// It takes it over before the image is reported as loaded, which may start another load.
				var adopted = false;
				Action<IImageSourceServiceResult>? onKeptImageReplaced = !keepPrevious
					? null
					: replacement =>
					{
						adopted = true;
						_isLoadedImageShown = true;
						OwnShownResult(replacement, platformView);
					};
#endif
				// The image is registered as shown before it is reported as loaded, which may start another load
				// (that keeps it, or releases it).
#if !ANDROID
				var registered = false;
#endif
				Action<IImageSourceServiceResult> onImageShown = shownResult =>
				{
					_isLoadedImageShown = true;
#if ANDROID
					if (adopted)
						return;
#else
					registered = true;
#endif
					SourceManager.SetResult(shownResult);
				};
				Action<Exception>? onFailed = errorSource is null
					? null
					: _ => ShowFallbackAsync(errorSource, _imageSourceServiceProvider, platformView, fallbackGeneration, () => true, fallbackToken).FireAndForget();
#endif

#if IOS
				var scale = handler.MauiContext?.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
				_decodedFor = null;
				var decodeOptions = GetDecodeOptions(platformView, scale, handler);
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, scale, token, onFailed, decodeOptions, onImageShown);
#elif WINDOWS
				var scale = handler.MauiContext?.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, scale, token, onFailed, onImageShown);
#elif ANDROID
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, token, onFailed, onKeptImageReplaced, onImageShown);
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
					// The newer load released (or kept) the image if it was shown; otherwise it was never shown.
					// (On Android, Glide manages the lifetime of images it set on the view.)
					if (!registered)
						result?.Dispose();
#endif
					return;
				}

				_isLoadedImageShown = result is not null;
#if IOS
				_decodedFor = result is not null && decodeOptions?.WasDownsampled == true ? decodeOptions : null;
#endif

#if ANDROID
				if (adopted)
				{
					// The loader owns this image already (see above).
					SourceManager.CompleteLoad(null);
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
				// Handled below like a fallback image that does not exist.
			}

			if (!IsCurrent())
			{
				// The fallback was never shown.
				result?.Dispose();
				return;
			}

			if (image is null)
			{
				// The fallback image could not be loaded either: show nothing (not an image that is no longer current).
				result?.Dispose();
				Setter.SetImageSource(null);
				_isLoadedImageShown = false;
#if IOS
				_decodedFor = null;
#endif
				ReleaseOwnedResult();
				return;
			}

			Setter.SetImageSource(image);
			_isLoadedImageShown = false;
#if IOS
			_decodedFor = null;
#endif
			OwnShownResult(result, platformView);
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
#if ANDROID
			_ownedImageView = null;
			_ownedDrawable = null;
#endif
			previous?.Dispose();
		}

		/// <summary>
		/// Takes over the result of the image that is shown on <paramref name="platformView"/>; it is released once another image replaces it.
		/// </summary>
		void OwnShownResult(IDisposable? result, PlatformView platformView)
		{
			ReplaceOwnedResult(result);
#if ANDROID
			if (result is not null && platformView is global::Android.Widget.ImageView imageView)
			{
				_ownedImageView = imageView;
				_ownedDrawable = imageView.Drawable;
			}
#endif
		}

		void ReleaseOwnedResult() =>
			ReplaceOwnedResult(null);

#if !ANDROID
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Uses instance data on Android.")]
#endif
		bool CanKeepPreviousImage(IElementHandler handler, IImageSource source)
		{
#if ANDROID
			// Loading into the view clears it, so the image is loaded without the view first; only the built-in services
			// are known to load images the same way without the view (others may override how they load into the view).
			_imageSourceServiceProvider ??= handler.GetRequiredService<IImageSourceServiceProvider>();
			var serviceType = _imageSourceServiceProvider.GetImageSourceService(source)?.GetType();
			return serviceType == typeof(FileImageSourceService) || serviceType == typeof(UriImageSourceService) ||
				serviceType == typeof(StreamImageSourceService) || serviceType == typeof(FontImageSourceService);
#else
			return true;
#endif
		}

#if ANDROID
		// The view that shows the owned image, and the image (the handler's PlatformView is cleared before it is disconnected).
		global::Android.Widget.ImageView? _ownedImageView;
		PlatformImage? _ownedDrawable;

		/// <summary>
		/// Stops showing the owned image: Glide may reuse its bitmap as soon as it is released.
		/// </summary>
		void RemoveOwnedImageFromView()
		{
			if (_ownedImageView is { } imageView && imageView.IsAlive() && _ownedDrawable is not null && imageView.Drawable == _ownedDrawable)
				imageView.SetImageDrawable(null);
		}
#endif
#endif

#if IOS
		// The size the image that is shown was decoded for, when it was decoded at a smaller size than its full size.
		ImageDecodeOptions? _decodedFor;
		bool _isRedecoding;

		/// <summary>
		/// Gets the size the image of an <see cref="IImage"/> will be displayed at, so that a large image can be decoded at that size.
		/// </summary>
		/// <remarks>With <paramref name="useBounds"/>, the view's current size (after layout) is used, rather than the size the new image is expected to have.</remarks>
		ImageDecodeOptions? GetDecodeOptions(PlatformView platformView, float density, IElementHandler handler, bool useBounds = false)
		{
			if (!RuntimeFeature.IsImageDownsamplingEnabled || platformView is not MauiImageView || Setter.ImageSourcePart is not IImage image)
				return null;

			var bounds = platformView.Bounds.Size;
			if (useBounds)
			{
				return bounds.Width > 0 && bounds.Height > 0
					? new ImageDecodeOptions(bounds.Width * density, bounds.Height * density, image.Aspect, isViewSizeKnown: true)
					: null;
			}

			// The size of the view only does not depend on the image when it is requested explicitly.
			if (Primitives.Dimension.IsExplicitSet(image.Width) && Primitives.Dimension.IsExplicitSet(image.Height) && image.Width > 0 && image.Height > 0)
				return new ImageDecodeOptions(image.Width * density, image.Height * density, image.Aspect, isViewSizeKnown: true);

			// Otherwise the current size may be the size of the previous image. The image is rarely shown larger than the window
			// (or the view is already larger); if it is, it is decoded again once laid out.
			var window = platformView.Window ?? handler.MauiContext?.GetOptionalPlatformWindow();
			if (window is null || window.Bounds.Width <= 0 || window.Bounds.Height <= 0)
				return null;

			var width = Math.Max(window.Bounds.Width, bounds.Width);
			var height = Math.Max(window.Bounds.Height, bounds.Height);
			return new ImageDecodeOptions(width * density, height * density, image.Aspect, isViewSizeKnown: false);
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

			// Only sources that can be decoded again without loading them again (which might return another image).
			var source = Setter.ImageSourcePart?.Source;
			if (source is not (IUriImageSource { CachingEnabled: true } or IFileImageSource))
				return;

			var density = platformView.Window.GetDisplayDensity();
			var required = GetDecodeOptions(platformView, density, handler, useBounds: true);
			if (required is null || !decodedFor.IsInsufficientFor(required))
				return;

			var options = new ImageDecodeOptions(required.PixelWidth, required.PixelHeight, required.Aspect, isViewSizeKnown: true)
			{
				FromCacheOnly = true,
				ContentVersion = decodedFor.ContentVersion,
			};
			RedecodeAsync(source, density, options).FireAndForget();
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

			var isCurrent = generation == _fallbackGeneration && !token.IsCancellationRequested && !SourceManager.IsLoading &&
				Setter.ImageSourcePart?.Source == source && _decodedFor is not null;

			// Only replace the image when it is still the one this was decoded for.
			if (result?.Value is not { } image || !isCurrent)
			{
				result?.Dispose();

				// The image is no longer cached (or could not be decoded): keep the image that is shown, and stop trying.
				if (result is null && isCurrent)
					_decodedFor = null;
			}
			else
			{
				_decodedFor = options.WasDownsampled ? options : null;
				Setter.SetImageSource(image);
				SourceManager.ReplaceResult(result);
			}

			// The view may have changed again while the image was decoded.
			if (result is not null)
				UpdateDecodedSize();
		}
#endif
	}
}
