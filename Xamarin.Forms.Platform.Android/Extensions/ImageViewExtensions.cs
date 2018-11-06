using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		public static Task UpdateBitmap(this AImageView imageView, IImageController newView, IImageController previousView) =>
			imageView.UpdateBitmap(newView, previousView, null, null);

		public static Task UpdateBitmap(this AImageView imageView, ImageSource newImageSource, ImageSource previousImageSourc) =>
			imageView.UpdateBitmap(null, null, newImageSource, previousImageSourc);

		// TODO hartez 2017/04/07 09:33:03 Review this again, not sure it's handling the transition from previousImage to 'null' newImage correctly
		static async Task UpdateBitmap(
			this AImageView imageView,
			IImageController newView,
			IImageController previousView,
			ImageSource newImageSource,
			ImageSource previousImageSource)
		{

			newImageSource = newView?.Source;
			previousImageSource = previousView?.Source;

			if (newImageSource == null || imageView.IsDisposed())
				return;

			if (Equals(previousImageSource, newImageSource))
				return;

			newView?.SetIsLoading(true);

			(imageView as IImageRendererController)?.SkipInvalidate();
			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			bool setByImageViewHandler = false;
			Bitmap bitmap = null;

			try
			{
				if (newImageSource != null)
				{
					var imageViewHandler = Internals.Registrar.Registered.GetHandlerForObject<IImageViewHandler>(newImageSource);
					if (imageViewHandler != null)
					{
						await imageViewHandler.LoadImageAsync(newImageSource, imageView);
						setByImageViewHandler = true;
					}
					else
					{
						var imageSourceHandler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(newImageSource);
						bitmap = await imageSourceHandler.LoadImageAsync(newImageSource, imageView.Context);
					}
				}
			}
			catch (TaskCanceledException)
			{
				newView?.SetIsLoading(false);
			}

			// Check if the source on the new image has changed since the image was loaded
			if (!Equals(newView?.Source, newImageSource))
			{
				bitmap?.Dispose();
				return;
			}

			if (!setByImageViewHandler && !imageView.IsDisposed())
			{
				if (bitmap == null && newImageSource is FileImageSource)
					imageView.SetImageResource(ResourceManager.GetDrawableByName(((FileImageSource)newImageSource).File));
				else
					imageView.SetImageBitmap(bitmap);
			}

			bitmap?.Dispose();
			newView?.SetIsLoading(false);
		}
	}
}
