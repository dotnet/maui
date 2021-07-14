#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;

namespace Microsoft.Maui
{

	public static class ImageViewExtensions
	{

		public static void Clear(this ImageView imageView)
		{ }

		public static void UpdateAspect(this ImageView imageView, IImage image)
		{ }

		public static void UpdateIsAnimationPlaying(this ImageView imageView, IImageSourcePart image)
		{
			if (image.IsAnimationPlaying)
			{
				;
			}
			else
			{
				;
			}
		}

		public static async Task<IImageSourceServiceResult<Gdk.Pixbuf>?> UpdateSourceAsync(this ImageView imageView, IImageSourcePart image, IImageSourceServiceProvider services, CancellationToken cancellationToken = default)
		{
			imageView.Clear();

			return await UpdateImageSourceAsync(image, (float)imageView.ScaleFactor, services, p =>
			{
				imageView.Image = p;
				imageView.UpdateIsAnimationPlaying(image);
			}, cancellationToken);

		}

		public static async Task<IImageSourceServiceResult<Gdk.Pixbuf>?> UpdateImageSourceAsync(this IImageSourcePart? image, float scale, IImageSourceServiceProvider provider, Action<Gdk.Pixbuf?> onResult, CancellationToken cancellationToken = default)
		{
			if (image == null)
				return null;

			image.UpdateIsLoading(false);

			var imageSource = image.Source;

			if (imageSource == null)
				return null;

			var events = image as IImageSourcePartEvents;

			image.UpdateIsLoading(true);

			try
			{
				var service = provider.GetRequiredImageSourceService(imageSource);
				var result = await service.GetImageAsync(imageSource, scale, cancellationToken);
				var pixbuf = result?.Value;
				var applied = !cancellationToken.IsCancellationRequested && imageSource == image.Source;

				// only set the image if we are still on the same one
				if (applied)
				{
					onResult(pixbuf);

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

		public static async Task<IImageSourceServiceResult<Gdk.Pixbuf>?> UpdateImageSourceAsync(this IImageSource? imageSource, float scale, IImageSourceServiceProvider provider, Action<Gdk.Pixbuf?> onResult, CancellationToken cancellationToken = default)
		{

			if (imageSource == null)
				return null;

			try
			{
				var service = provider.GetRequiredImageSourceService(imageSource);
				var result = await service.GetImageAsync(imageSource, scale, cancellationToken);
				var pixbuf = result?.Value;
				var applied = !cancellationToken.IsCancellationRequested;

				if (applied)
				{
					onResult(pixbuf);

				}

				return result;
			}
			catch (OperationCanceledException)
			{
				// no-op
			}
			catch (Exception)
			{
				// no-op

			}

			return null;

		}

		public static TempFile? TempFileFor(this Gdk.Pixbuf? p, ImageFormat format = ImageFormat.Png)
		{
			if (p == null)
				return default;

			var tmpfile = new TempFile();

			p.Save(tmpfile, format.ToImageExtension());

			return tmpfile;
		}

	}

}