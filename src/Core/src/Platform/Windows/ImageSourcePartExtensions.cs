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
			Action<Exception>? onFailed = null)
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