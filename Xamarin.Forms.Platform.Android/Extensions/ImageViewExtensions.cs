using System;
using System.Threading.Tasks;
using Android.Graphics;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		// TODO hartez 2017/04/07 09:33:03 Review this again, not sure it's handling the transition from previousImage to 'null' newImage correctly
		public static async Task UpdateBitmap(this AImageView imageView, Image newImage, Image previousImage = null)
		{
			if (imageView == null || imageView.IsDisposed())
				return;

			if (Device.IsInvokeRequired)
				throw new InvalidOperationException("Image Bitmap must not be updated from background thread");

			if (previousImage != null && Equals(previousImage.Source, newImage.Source))
				return;

			var imageController = newImage as IImageController;

			imageController?.SetIsLoading(true);

			(imageView as IImageRendererController)?.SkipInvalidate();

			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			ImageSource source = newImage?.Source;
			Bitmap bitmap = null;
			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				try
				{
					bitmap = await handler.LoadImageAsync(source, imageView.Context);
				}
				catch (TaskCanceledException)
				{
					imageController?.SetIsLoading(false);
				}
			}

			if (newImage == null || !Equals(newImage.Source, source))
			{
				bitmap?.Dispose();
				return;
			}

			if (!imageView.IsDisposed())
			{
				if (bitmap == null && source is FileImageSource)
					imageView.SetImageResource(ResourceManager.GetDrawableByName(((FileImageSource)source).File));
				else
				{
					imageView.SetImageBitmap(bitmap);
				}
			}

			bitmap?.Dispose();

			imageController?.SetIsLoading(false);
			((IVisualElementController)newImage).NativeSizeChanged();
		}
	}
}
