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
			ReleaseFallback();
#endif
		}

		public async Task UpdateImageSourceAsync()
		{
			if (Setter.Handler is not IElementHandler handler || handler.PlatformView is not PlatformView platformView)
			{
				return;
			}

			var token = SourceManager.BeginLoad();
			var imageSource = Setter.ImageSourcePart;

			if (imageSource?.Source is not null)
			{
#if IOS || ANDROID || WINDOWS
				_imageSourceServiceProvider ??= handler.GetRequiredService<IImageSourceServiceProvider>();

				var fallbacks = imageSource as IImageSourcePartFallbacks;
				var errorSource = fallbacks?.ErrorSource;
				Action<Exception>? onFailed = errorSource is null
					? null
					: _ => ShowFallbackAsync(errorSource, _imageSourceServiceProvider, platformView, token, () => true).FireAndForget();
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
					ShowFallbackAsync(placeholderSource, _imageSourceServiceProvider, platformView, token, () => !loadTask.IsCompleted)
						.FireAndForget();
				}

				var result = await loadTask;

				// A newer load has started (or the loader was reset); it owns the result and the fallbacks now.
				if (token.IsCancellationRequested)
					return;

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
				Setter.SetImageSource(null);
#if IOS || ANDROID || WINDOWS
				ReleaseFallback();
#endif
#if ANDROID
				CheckForImageLoadedOnAttached = false;
#endif
			}
		}

#if IOS || ANDROID || WINDOWS
		IDisposable? _fallbackResult;

		/// <summary>
		/// Shows a placeholder or error image without changing the loading state.
		/// </summary>
		async Task ShowFallbackAsync(IImageSource source, IImageSourceServiceProvider provider, PlatformView platformView, CancellationToken token, Func<bool> isStillNeeded)
		{
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
			catch (Exception) when (token.IsCancellationRequested)
			{
				return;
			}
			catch (Exception)
			{
				// The fallback image could not be loaded either: show nothing.
				if (!token.IsCancellationRequested && isStillNeeded())
				{
					Setter.SetImageSource(null);
					ReleaseFallback();
				}
				return;
			}

			if (token.IsCancellationRequested || !isStillNeeded() || image is null)
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

		void ReleaseFallback()
		{
			var previous = _fallbackResult;
			_fallbackResult = null;
			previous?.Dispose();
		}
#endif
	}
}
