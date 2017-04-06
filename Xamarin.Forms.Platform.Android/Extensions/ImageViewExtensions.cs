using System;
using System.Threading.Tasks;
using Android.Graphics;
using Java.IO;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		public static async void UpdateBitmap(this AImageView imageView, Image newImage, Image previousImage = null)
		{
			if (Device.IsInvokeRequired)
				throw new InvalidOperationException("Image Bitmap must not be updated from background thread");

			if (previousImage != null && Equals(previousImage.Source, newImage.Source))
				return;

			((IImageController)newImage).SetIsLoading(true);

			(imageView as IImageRendererController).SkipInvalidate();

			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			ImageSource source = newImage.Source;
			Bitmap bitmap = null;
			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
			{
				try
				{
					bitmap = await handler.LoadImageAsync(source, imageView.Context);
				}
				catch (TaskCanceledException)
				{
				}
				catch (IOException ex)
				{
					Internals.Log.Warning("Xamarin.Forms.Platform.Android.ImageRenderer", "Error updating bitmap: {0}", ex);
				}
			}

			if (newImage == null || !Equals(newImage.Source, source))
			{
				bitmap?.Dispose();
				return;
			}

			if (bitmap == null && source is FileImageSource)
				imageView.SetImageResource(ResourceManager.GetDrawableByName(((FileImageSource)source).File));
			else
				imageView.SetImageBitmap(bitmap);

			bitmap?.Dispose();

			((IImageController)newImage).SetIsLoading(false);
			((IVisualElementController)newImage).NativeSizeChanged();
		}
	}
}
