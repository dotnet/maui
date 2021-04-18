﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
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

		public static void UpdateIsAnimationPlaying(this ImageView imageView, IImageSourcePart image)
		{
			if (imageView.Drawable is IAnimatable animatable)
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

		public static async Task UpdateSourceAsync(this ImageView imageView, IImageSourcePart image, IImageSourceServiceProvider services, CancellationToken cancellationToken = default)
		{
			imageView.Clear();

			image.UpdateIsLoading(false);

			var context = imageView.Context;
			if (context == null)
				return;

			var imageSource = image.Source;
			if (imageSource == null)
				return;

			var events = image as IImageSourcePartEvents;

			events?.LoadingStarted();
			image.UpdateIsLoading(true);

			try
			{
				var service = services.GetRequiredImageSourceService(imageSource);
				if (service is IApplyImageSourceService applyService)
				{
					// use the faster/better way

					var applied = await applyService.ApplyDrawableAsync(imageSource, image, imageView, cancellationToken);

					events?.LoadingCompleted(applied);
				}
				else
				{
					// fall back to setting it manually

					var drawable = await service.GetDrawableAsync(imageSource, context, cancellationToken);

					var applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;

					// only set the image if we are still on the same one
					if (applied)
					{
						imageView.SetImageDrawable(drawable);

						if (drawable is IAnimatable animatable && image.IsAnimationPlaying)
							animatable.Start();
					}

					events?.LoadingCompleted(applied);
				}
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
		}
	}
}