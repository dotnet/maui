#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui
{
	public static class ImageViewExtensions
	{
		public static void Clear(this ImageView imageView)
		{
			// stop the animation
			if (imageView.Drawable is IAnimatable animatable)
				animatable.Stop();

			// clear the view and release any bitmaps
			imageView.SetImageResource(global::Android.Resource.Color.Transparent);
		}

		public static void UpdateAspect(this ImageView imageView, IImage image)
		{
			imageView.SetScaleType(image.Aspect.ToScaleType());
		}

		public static void UpdateIsAnimationPlaying(this ImageView imageView, IImageSourcePart image) =>
			imageView.Drawable.UpdateIsAnimationPlaying(image);

		public static void UpdateIsAnimationPlaying(this Drawable? drawable, IImageSourcePart image)
		{
			if (drawable is IAnimatable animatable)
			{
				if (image.IsAnimationPlaying)
				{
					if (!animatable.IsRunning)
						animatable.Start();
				}
				else
				{
					if (animatable.IsRunning)
						animatable.Stop();
				}
			}
		}

		public static Task<IImageSourceServiceResult<Drawable>?> UpdateSourceAsync(
			this ImageView imageView,
			IImageSourcePart image,
			IImageSourceServiceProvider services,
			CancellationToken cancellationToken = default)
		{
			imageView.Clear();
			return image.UpdateSourceAsync(imageView, services, (d) => imageView.SetImageDrawable(d), cancellationToken);
		}

		internal static async Task<IImageSourceServiceResult<Drawable>?> UpdateSourceAsync(
			this IImageSourcePart image,
			View destinationContext,
			IImageSourceServiceProvider services,
			Action<Drawable?> setDrawable,
			CancellationToken cancellationToken = default)
		{
			image.UpdateIsLoading(false);

			var context = destinationContext.Context;
			if (context == null)
				return null;

			var imageSource = image.Source;
			if (imageSource == null)
				return null;

			var events = image as IImageSourcePartEvents;

			events?.LoadingStarted();
			image.UpdateIsLoading(true);

			try
			{
				var service = services.GetRequiredImageSourceService(imageSource);

				var result = await service.GetDrawableAsync(imageSource, context, cancellationToken);
				var drawable = result?.Value;

				var applied = !cancellationToken.IsCancellationRequested && destinationContext.IsAlive() && imageSource == image.Source;

				// only set the image if we are still on the same one
				if (applied)
				{
					setDrawable(drawable);
					drawable.UpdateIsAnimationPlaying(image);
				}

				events?.LoadingCompleted(applied);

				return result;
			}
			catch (OperationCanceledException)
			{
				// no-op
				events?.LoadingCompleted(false);
			}
			catch (Exception ex)
			{
				events?.LoadingFailed(ex);
			}
			finally
			{
				// only mark as finished if we are still working on the same image
				if (imageSource == image.Source)
				{
					image.UpdateIsLoading(false);
				}
			}

			return null;
		}
	}
}