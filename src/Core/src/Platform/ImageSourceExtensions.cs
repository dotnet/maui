using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#if __IOS__ || MACCATALYST
using NativeImage = UIKit.UIImage;
#elif MONOANDROID
using NativeImage = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using NativeImage = Microsoft.UI.Xaml.Media.ImageSource;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeImage = System.Object;
#endif

namespace Microsoft.Maui
{
	public static class ImageSourceExtensions
	{
		public static Task<IImageSourceServiceResult<NativeImage>?> GetNativeImage(this IImageSource imageSource, IMauiContext mauiContext)
		{
			var services = mauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = provider.GetRequiredImageSourceService(imageSource);
			return imageSource.GetNativeImage(mauiContext, imageSourceService);
		}

		public static Task<IImageSourceServiceResult<NativeImage>?> GetNativeImage(this IImageSource imageSource, IMauiContext mauiContext, IImageSourceService imageSourceService)
		{
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
