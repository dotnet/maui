using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class ImageSourcePartExtensions
	{
		public static async Task<IImageSourceServiceResult<UIImage>?> UpdateSourceAsync(
			this IImageSourcePart image,
			UIView destinationContext,
			IImageSourceServiceProvider services,
			Action<UIImage?> setImage,
			float imageScale = 1.0f,
			CancellationToken cancellationToken = default,
			Action<Exception>? onFailed = null,
			ImageDecodeOptions? decodeOptions = null,
			Action<IImageSourceServiceResult>? onImageShown = null)
		{
			image.UpdateIsLoading(false);

			var imageSource = image.Source;
			if (imageSource == null)
				return null;

			var events = image as IImageSourcePartEvents;

			events?.LoadingStarted();
			image.UpdateIsLoading(true);

			try
			{
				var service = services.GetRequiredImageSourceService(imageSource);

				var result = decodeOptions is not null && service is IImageSourceServiceWithDecodeOptions decodingService
					? await decodingService.GetImageAsync(imageSource, imageScale, decodeOptions, cancellationToken)
					: await service.GetImageAsync(imageSource, imageScale, cancellationToken);
				var uiImage = result?.Value;

				var applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;

				// only set the image if we are still on the same one
				if (applied)
				{
					setImage.Invoke(uiImage);
					if (destinationContext is UIImageView imageView)
						imageView.UpdateIsAnimationPlaying(image);
				}

				// Setting the image runs mappers (such as IsAnimationPlaying), which may have started a newer load.
				applied = applied && !cancellationToken.IsCancellationRequested && imageSource == image.Source;

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
			catch (Exception) when (cancellationToken.IsCancellationRequested)
			{
				// The load was cancelled (for example, the handler was disconnected), but the
				// image source service reported it as a failure; treat it as a cancellation
				events?.LoadingCompleted(false);
			}
			catch (Exception) when (imageSource != image.Source)
			{
				// A newer source is loading; this failure is no longer relevant.
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
	}
}