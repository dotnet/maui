#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui
{
	public static class ImageViewExtensions
	{
		const string BitmapImageTypeName = "Microsoft.UI.Xaml.Media.Imaging.BitmapImage";

		static bool IsAnimationSupported;

		static ImageViewExtensions()
		{
			IsAnimationSupported =
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.IsAnimatedBitmap)) &&
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.IsPlaying)) &&
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.Play)) &&
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.Stop));
		}

		public static void Clear(this WImage imageView)
		{
			imageView.Source = null;
		}

		public static void UpdateAspect(this WImage imageView, IImage image)
		{
			imageView.Stretch = image.Aspect.ToStretch();
		}

		public static void UpdateIsAnimationPlaying(this WImage imageView, IImageSourcePart image)
		{
			if (!IsAnimationSupported)
				return;

			if (imageView.Source is BitmapImage bitmapImage && bitmapImage.IsAnimatedBitmap)
			{
				if (image.IsAnimationPlaying)
				{
					if (!bitmapImage.IsPlaying)
						bitmapImage.Play();
				}
				else
				{
					if (bitmapImage.IsPlaying)
						bitmapImage.Stop();
				}
			}
		}

		public static async Task<IImageSourceServiceResult<WImageSource>?> UpdateSourceAsync(this WImage imageView, IImageSourcePart image, IImageSourceServiceProvider services, CancellationToken cancellationToken = default)
		{
			imageView.Clear();

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

				var scale = imageView.XamlRoot?.RasterizationScale ?? 1;
				var result = await service.GetImageSourceAsync(imageSource, (float)scale, cancellationToken);
				var uiImage = result?.Value;

				var applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;

				// only set the image if we are still on the same one
				if (applied)
				{
					imageView.Source = uiImage;

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