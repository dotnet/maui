using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#if __IOS__ || MACCATALYST
using PlatformImage = UIKit.UIImage;
#elif MONOANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformImage = System.Object;
#endif

namespace Microsoft.Maui
{
	public static class ImageSourceExtensions
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

#if __IOS__ || MACCATALYST
			return imageSourceService.GetImageAsync(imageSource);
#elif MONOANDROID
			return imageSourceService.GetDrawableAsync(imageSource, mauiContext.Context!);
#elif WINDOWS
			return imageSourceService.GetImageSourceAsync(imageSource);
#else
			throw new NotImplementedException();
#endif
		}
	}
}
