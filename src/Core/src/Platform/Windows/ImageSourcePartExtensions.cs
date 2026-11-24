using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Platform
{
	internal static class ImageSourcePartExtensions
	{
		public static async Task<IImageSourceServiceResult<WImageSource>?> UpdateSourceAsync(
			this IImageSourcePart image,
			FrameworkElement destinationContext,
			IImageSourceServiceProvider services,
			Action<WImageSource?> setImage,
			float imageScale = 1.0f,
			CancellationToken cancellationToken = default,
			Action<Exception>? onFailed = null,
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

				var result = await service.GetImageSourceAsync(imageSource, imageScale, cancellationToken);
				var uiImage = result?.Value;

				var applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;

				// only set the image if we are still on the same one
				if (applied)
				{
					setImage(uiImage);
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