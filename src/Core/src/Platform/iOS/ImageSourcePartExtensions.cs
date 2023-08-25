using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class ImageSourcePartExtensions
	{
		public static async Task<IImageSourceServiceResult<UIImage>?> UpdateSourceAsync(
			this IImageSourcePart image,
			UIView destinationContext,
			IMauiContext? mauiContext,
			Action<UIImage?> setImage,
			CancellationToken cancellationToken = default)
		{
			image.UpdateIsLoading(false);

			var imageSource = image.Source;
			if (imageSource == null)
				return null;

			if (mauiContext == null)
				return null;

			var events = image as IImageSourcePartEvents;

			events?.LoadingStarted();
			image.UpdateIsLoading(true);

			try
			{
				var imageSourceServiceProvider = mauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
				var imageSourceService = imageSourceServiceProvider.GetRequiredImageSourceService(imageSource);

				var scale = mauiContext.GetOptionalPlatformWindow()?.GetDisplayDensity() ?? 1;
				var result = await imageSourceService.GetImageAsync(imageSource, (float)scale, cancellationToken);
				var uiImage = result?.Value;

				var applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;

				// only set the image if we are still on the same one
				if (applied)
				{
					setImage.Invoke(uiImage);
					if (destinationContext is UIImageView imageView)
						imageView.UpdateIsAnimationPlaying(image);
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