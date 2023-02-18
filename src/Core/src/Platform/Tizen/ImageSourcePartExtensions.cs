using System;
using System.Threading;
using System.Threading.Tasks;
using NView = Tizen.NUI.BaseComponents.View;
using TImage = Tizen.NUI.BaseComponents.ImageView;

namespace Microsoft.Maui.Platform
{
	public static class ImageSourcePartExtensions
	{
		public static async Task<IImageSourceServiceResult<MauiImageSource>?> UpdateSourceAsync(this IImageSourcePart image, NView destinationContext, IImageSourceServiceProvider services, Action<MauiImageSource?> setImage, CancellationToken cancellationToken = default)
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
				var result = await service.GetImageAsync(imageSource, cancellationToken);
				var tImage = result?.Value;

				var applied = !cancellationToken.IsCancellationRequested && tImage != null && imageSource == image.Source;

				if (applied)
				{
					if (destinationContext is TImage platformImageView)
					{
						TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

						void completed(object? sender, EventArgs args)
						{
							tcs.SetResult(platformImageView.LoadingStatus == TImage.LoadingStatusType.Ready);
						}
						try
						{
							platformImageView.ResourceReady += completed;
							setImage.Invoke(tImage);
							await tcs.Task;
						}
						finally
						{
							platformImageView.ResourceReady -= completed;
						}
					}
					else
					{
						setImage.Invoke(tImage);
					}
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
