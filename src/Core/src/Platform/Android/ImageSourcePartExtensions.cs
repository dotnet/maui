using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	internal static class ImageSourcePartExtensions
	{
		public static async Task<IImageSourceServiceResult?> UpdateSourceAsync(
			this IImageSourcePart image,
			View destinationContext,
			IImageSourceServiceProvider services,
			Action<Drawable?> setImage,
			CancellationToken cancellationToken = default,
			Action<Exception>? onFailed = null,
			Action<IImageSourceServiceResult>? onKeptImageReplaced = null,
			Action<IImageSourceServiceResult>? onImageShown = null)
		{
			image.UpdateIsLoading(false);

			var context = destinationContext.Context;
			if (context == null)
				return null;

			var destinationImageView = destinationContext as ImageView;

			if (destinationImageView is null && setImage is null)
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

				var applied = !cancellationToken.IsCancellationRequested && destinationContext.IsAlive() && imageSource == image.Source;

				IImageSourceServiceResult? result = null;

				if (applied)
				{
					// Keeping the image that is shown while the new one loads.
					if (destinationImageView is not null && onKeptImageReplaced is not null && setImage is not null)
					{
						// Loading into the view would clear the image that is shown right away, so load the new image
						// for the view's size without touching the view, then replace the image in one step.
						result = await GetDrawableForViewAsync(service, imageSource, destinationImageView, context, cancellationToken);
						applied = !cancellationToken.IsCancellationRequested && destinationContext.IsAlive() && imageSource == image.Source;
						if (applied && result is IImageSourceServiceResult<Drawable> drawableResult)
						{
							// Stop the view's own load (if any), so that it cannot replace this image later.
							PlatformInterop.ClearImageView(destinationImageView);
							setImage.Invoke(drawableResult.Value);
							destinationImageView.UpdateIsAnimationPlaying(image);
							onKeptImageReplaced(drawableResult);
						}
					}
					else if (destinationImageView is not null)
					{
						result = await service.LoadDrawableAsync(imageSource, destinationImageView, cancellationToken);
						applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;
					}
					else
					{
						result = await service.GetDrawableAsync(imageSource, context, cancellationToken);

						// Only set the image if we are still on the same one.
						applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;
						if (applied && setImage is not null && result is IImageSourceServiceResult<Drawable> drawableResult)
							setImage.Invoke(drawableResult.Value);
					}

					if (result is null)
						throw new InvalidOperationException("Glide failed to load image");
				}

				// The image is shown before that is reported (which may start another load).
				if (applied && result is not null)
					onImageShown?.Invoke(result);

				events?.LoadingCompleted(applied);

				if (!applied)
				{
					// The image was never shown.
					result?.Dispose();
					return null;
				}

				return result;
			}
			catch (OperationCanceledException)
			{
				// no-op
				events?.LoadingCompleted(false);
			}
			catch (Exception) when (cancellationToken.IsCancellationRequested || imageSource != image.Source)
			{
				// The load was cancelled (for example, the handler was disconnected) or a newer source is loading,
				// but the image source service reported it as a failure. Treat it as a cancellation so that it
				// does not clear the newer image.
				events?.LoadingCompleted(false);
			}
			catch (Exception ex)
			{
				if (onFailed is not null)
					onFailed(ex);
				else
					setImage?.Invoke(null);
				events?.LoadingFailed(ex);
			}
			finally
			{
				// only mark as finished if we are still working on the same image (a newer load,
				// including a reload of the same source, cancels this one and reports its own state)
				if (imageSource == image.Source && !cancellationToken.IsCancellationRequested)
				{
					image.UpdateIsLoading(false);
				}
			}

			return null;
		}

		static Task<IImageSourceServiceResult<Drawable>?> GetDrawableForViewAsync(IImageSourceService service, IImageSource imageSource, ImageView imageView, global::Android.Content.Context context, CancellationToken cancellationToken)
		{
			if (service is not IViewSizedImageSourceService sizedService)
				return service.GetDrawableAsync(imageSource, context, cancellationToken);

			// The same size that a load into the view would use (see Glide's ViewTarget.SizeDeterminer).
			var width = GetTargetSize(imageView.Width, imageView.LayoutParameters?.Width ?? 0, imageView.PaddingLeft + imageView.PaddingRight, context);
			var height = GetTargetSize(imageView.Height, imageView.LayoutParameters?.Height ?? 0, imageView.PaddingTop + imageView.PaddingBottom, context);

			return sizedService.GetDrawableAsync(imageSource, context, width, height, cancellationToken);
		}

		static int GetTargetSize(int viewSize, int layoutParamSize, int padding, global::Android.Content.Context context)
		{
			if (layoutParamSize - padding > 0)
				return layoutParamSize - padding;

			if (viewSize - padding > 0)
				return viewSize - padding;

			var metrics = context.Resources?.DisplayMetrics;
			return metrics is null
				? IViewSizedImageSourceService.SizeOriginal
				: Math.Max(metrics.WidthPixels, metrics.HeightPixels);
		}
	}
}
