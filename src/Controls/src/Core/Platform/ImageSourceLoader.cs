using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIImage;
#elif MONOANDROID
using NativeView = Android.Graphics.Drawable.Drawable;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Media.ImageSource;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Controls.Platform
{
	partial class ImageSourceLoader : IImageSourcePart
	{
		public IImageSource Source
		{
			get;
			set;
		}

		public IMauiContext MauiContext
		{
			get;
			set;
		}

		public bool IsAnimationPlaying { get; set; }
		public bool IsLoading { get; private set; }

		public void UpdateIsLoading(bool isLoading)
		{
			IsLoading = isLoading;
		}

		public static Task<IImageSourceServiceResult<NativeView>> GetImageAsync(IImageSource imageSource, IMauiContext mauiContext)
		{
			if (imageSource == null)
				return Task.FromResult<IImageSourceServiceResult<NativeView>>(new ImageSourceServiceResult(null));

			var services = mauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = provider.GetRequiredImageSourceService(imageSource);
			return GetNativeImage(imageSource, imageSourceService, mauiContext);
		}

		public static Task<IImageSourceServiceResult<NativeView>> GetImageAsync(IImageSourcePart imagePart, IMauiContext mauiContext)
		{
			var services = mauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = provider.GetRequiredImageSourceService(imagePart.Source);
			return GetNativeImage(imagePart.Source, imageSourceService, mauiContext);
		}

		static async Task LoadImageResult(Task<IImageSourceServiceResult<NativeView>> task, Action<IImageSourceServiceResult<NativeView>> finished = null)
		{
			var result = await task;
			finished?.Invoke(result);
		}

		public static void LoadImage(IImageSource source, IMauiContext mauiContext, Action<IImageSourceServiceResult<NativeView>> finished = null)
		{
			LoadImageResult(GetImageAsync(source, mauiContext), finished)
						.FireAndForget(e => Internals.Log.Warning(nameof(ImageSourceLoader), $"{e}"));
		}

		public static void LoadImage(IImageSourcePart part, IMauiContext mauiContext, Action<IImageSourceServiceResult<NativeView>> finished = null)
		{
			LoadImageResult(GetImageAsync(part.Source, mauiContext), finished)
						.FireAndForget(e => Internals.Log.Warning(nameof(ImageSourceLoader), $"{e}"));
		}
	}
}
