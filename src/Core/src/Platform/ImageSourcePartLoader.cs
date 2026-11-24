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
			CancelFallbacks();
			ReleaseFallback();
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
				Action<Exception>? onFailed = errorSource is null
					? null
					: _ => ShowFallbackAsync(errorSource, _imageSourceServiceProvider, platformView, fallbackGeneration, () => true, fallbackToken).FireAndForget();
#endif

#if IOS || WINDOWS
				var scale = handler.MauiContext?.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, scale, token, onFailed);
#elif ANDROID
				var loadTask = imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, Setter.SetImageSource, token, onFailed);
#endif

#if IOS || ANDROID || WINDOWS
				// Only show the placeholder when the image is not available right away (for example from a memory cache).
				if (!loadTask.IsCompleted && fallbacks?.PlaceholderSource is { } placeholderSource)
				{
					ShowFallbackAsync(placeholderSource, _imageSourceServiceProvider, platformView, fallbackGeneration, () => !loadTask.IsCompleted, fallbackToken)
						.FireAndForget();
				}

				var result = await loadTask;

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

				SourceManager.CompleteLoad(result);

				// Release the placeholder once the image replaced it. After a failure the
				// placeholder stays until the error image (if any) replaces it.
				if (result is not null || errorSource is null)
					ReleaseFallback();
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
				ReleaseFallback();
#endif
				SourceManager.CompleteLoad(null);

				// A load that was cancelled by clearing the source does not report that it stopped loading.
				imageSource?.UpdateIsLoading(false);
#if ANDROID
				CheckForImageLoadedOnAttached = false;
#endif
			}
		}

#if IOS || ANDROID || WINDOWS
		IDisposable? _fallbackResult;
		CancellationTokenSource? _fallbackCancellation;

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
					ReleaseFallback();
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

			var previous = _fallbackResult;
			_fallbackResult = result;
			previous?.Dispose();
		}

		void CancelFallbacks()
		{
			var cancellation = _fallbackCancellation;
			_fallbackCancellation = null;
			cancellation?.Cancel();
		}

		void ReleaseFallback()
		{
			var previous = _fallbackResult;
			_fallbackResult = null;
			previous?.Dispose();
		}
#endif
	}
}
