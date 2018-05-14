using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		// TODO hartez 2017/04/07 09:33:03 Review this again, not sure it's handling the transition from previousImage to 'null' newImage correctly
		public static async Task UpdateBitmap(this AImageView imageView, Image newImage, ImageSource source, Image previousImage = null, ImageSource previousImageSource = null)
		{
			if (imageView == null || imageView.IsDisposed())
				return;

			if (Device.IsInvokeRequired)
				throw new InvalidOperationException("Image Bitmap must not be updated from background thread");

			source = source ?? newImage?.Source;
			previousImageSource = previousImageSource ?? previousImage?.Source;

			if (Equals(previousImageSource, source))
				return;

			var imageController = newImage as IImageController;

			imageController?.SetIsLoading(true);

			(imageView as IImageRendererController)?.SkipInvalidate();

			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			Bitmap bitmap = null;
			Drawable drawable = null;

			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				if (handler is FileImageSourceHandler)
				{
					drawable = imageView.Context.GetDrawable((FileImageSource)source);
				}

				if (drawable == null)
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
			}

			// Check if the source on the new image has changed since the image was loaded
			if (newImage != null && !Equals(newImage.Source, source))
			{
				bitmap?.Dispose();
				return;
			}

			if (!imageView.IsDisposed())
			{
				if (bitmap == null && drawable != null)
				{
					imageView.SetImageDrawable(drawable);
				}
				else
				{
					imageView.SetImageBitmap(bitmap);
				}
			}

			bitmap?.Dispose();
			imageController?.SetIsLoading(false);
			((IVisualElementController)newImage)?.NativeSizeChanged();

		}

		public static async Task UpdateBitmap(this AImageView imageView, Image newImage, Image previousImage = null)
		{
			await UpdateBitmap(imageView, newImage, newImage?.Source, previousImage, previousImage?.Source);

		}
	}
}
