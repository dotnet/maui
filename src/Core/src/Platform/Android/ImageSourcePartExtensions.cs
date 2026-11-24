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
			Action<Exception>? onFailed = null)
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
					if (destinationImageView is not null)
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

				events?.LoadingCompleted(applied);
				return result;
			}
			catch (OperationCanceledException)
			{
				// no-op
				events?.LoadingCompleted(false);
			}
			catch (Exception) when (cancellationToken.IsCancellationRequested || imageSource != image.Source)
			{
				// The load was cancelled or a newer source is loading; this failure is no longer relevant
				// and must not clear the newer image.
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