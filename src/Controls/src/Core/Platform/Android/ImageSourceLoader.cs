using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Platform
{
	class ImageSourceLoader : IImageSourcePart
	{

		static Task<IImageSourceServiceResult<global::Microsoft.UI.Xaml.Media.ImageSource>> GetNativeImage(IImageSource imageSource, IImageSourceService imageSourceService, IMauiContext mauiContext)
		{
			return imageSourceService.GetImageSourceAsync(imageSource);
		}

		public void LoadImage(ImageView view, Action<IImageSourceServiceResult<Drawable>> finished = null)
		{
			LoadImageResult(LoadImageAsync(view), finished)
						.FireAndForget(e => Internals.Log.Warning(nameof(ImageSourceLoader), $"{e}"));
		}

		static async Task LoadImageResult(Task<IImageSourceServiceResult<Drawable>> task, Action<IImageSourceServiceResult<Drawable>> finished = null)
		{
			var result = await task;
			finished?.Invoke(result);
		}

		public Task<IImageSourceServiceResult<Drawable>> LoadImageAsync(ImageView view)
		{
			var services = MauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			return view.UpdateSourceAsync(this, provider);
		}

		public static Task<IImageSourceServiceResult<Drawable>> LoadImageAsync(ImageView imageView, IMauiContext mauiContext, IImageSource imageSource)
		{
			var part = new ImageSourceLoader()
			{
				MauiContext = mauiContext,
				Source = imageSource,
			};

			return LoadImageAsync(imageView, part, mauiContext);
		}

		public static Task<IImageSourceServiceResult<Drawable>> LoadImageAsync(ImageView imageView, IImageSourcePart part, IMauiContext mauiContext)
		{
			var services = mauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			return imageView.UpdateSourceAsync(part, provider);
		}
	}
}
