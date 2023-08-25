using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#if IOS || MACCATALYST
using PlatformImage = UIKit.UIImage;
#elif MONOANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
#elif TIZEN
using PlatformImage = Microsoft.Maui.Platform.MauiImageSource;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformImage = System.Object;
#endif

namespace Microsoft.Maui
{
	public static partial class ImageSourceExtensions
	{
		public static void LoadImage(this IImageSource? source, IMauiContext mauiContext, Action<IImageSourceServiceResult<PlatformImage>?>? finished = null)
		{
			LoadImageResult(source.GetPlatformImageAsync(mauiContext), finished)
						.FireAndForget(mauiContext.Services.CreateLogger<IImageSource>(), nameof(LoadImage));
		}

		static async Task LoadImageResult(Task<IImageSourceServiceResult<PlatformImage>?> task, Action<IImageSourceServiceResult<PlatformImage>?>? finished = null)
		{
			var result = await task;
			finished?.Invoke(result);
		}

		public static Task<IImageSourceServiceResult<PlatformImage>?> GetPlatformImageAsync(this IImageSource? imageSource, IMauiContext mauiContext)
		{
			if (imageSource == null)
				return Task.FromResult<IImageSourceServiceResult<PlatformImage>?>(null);

			var services = mauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = provider.GetRequiredImageSourceService(imageSource);
			return imageSourceService.GetPlatformImageAsync(imageSource, mauiContext);
		}

		public static Task<IImageSourceServiceResult<PlatformImage>?> GetPlatformImageAsync(this IImageSourceService imageSourceService, IImageSource? imageSource, IMauiContext mauiContext)
		{
			if (imageSource == null)
				return Task.FromResult<IImageSourceServiceResult<PlatformImage>?>(null);

#if IOS || MACCATALYST
			var scale = mauiContext.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
			return imageSourceService.GetImageAsync(imageSource, scale);
#elif ANDROID
			return imageSourceService.GetDrawableAsync(imageSource, mauiContext.Context!);
#elif WINDOWS
			var scale = mauiContext.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1.0f;
			return imageSourceService.GetImageSourceAsync(imageSource, scale);
#elif TIZEN
			return imageSourceService.GetImageAsync(imageSource);
#else
			throw new NotImplementedException();
#endif
		}
	}
}
