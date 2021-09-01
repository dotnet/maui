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


		public static Task<IImageSourceServiceResult<Drawable>> GetImageAsync(IImageSource imageSource, IMauiContext mauiContext)
		{
			if (imageSource == null)
				return Task.FromResult<IImageSourceServiceResult<Drawable>>(new ImageSourceServiceResult(null));

			var services = mauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = provider.GetRequiredImageSourceService(imageSource);
			return imageSourceService.GetDrawableAsync(imageSource, mauiContext.Context);
		}

		public static Task<IImageSourceServiceResult<Drawable>> GetImageAsync(IImageSourcePart imagePart, IMauiContext mauiContext)
		{
			var services = mauiContext.Services;
			var provider = services.GetRequiredService<IImageSourceServiceProvider>();
			var imageSourceService = provider.GetRequiredImageSourceService(imagePart.Source);
			return imageSourceService.GetDrawableAsync(imagePart.Source, mauiContext.Context);
		}

		public static void LoadImage(IImageSource source, IMauiContext mauiContext, Action<IImageSourceServiceResult<Drawable>> finished = null)
		{
			LoadImageResult(GetImageAsync(source, mauiContext), finished)
						.FireAndForget(e => Internals.Log.Warning(nameof(ImageSourceLoader), $"{e}"));
		}

		public static void LoadImage(IImageSourcePart part, IMauiContext mauiContext, Action<IImageSourceServiceResult<Drawable>> finished = null)
		{
			LoadImageResult(GetImageAsync(part.Source, mauiContext), finished)
						.FireAndForget(e => Internals.Log.Warning(nameof(ImageSourceLoader), $"{e}"));
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
