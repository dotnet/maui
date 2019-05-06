using System.Threading.Tasks;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		public static Task UpdateBitmap(this AImageView imageView, IImageElement newView, IImageElement previousView) =>
			imageView.UpdateBitmap(newView, previousView, null, null);

		public static Task UpdateBitmap(this AImageView imageView, ImageSource newImageSource, ImageSource previousImageSourc) =>
			imageView.UpdateBitmap(null, null, newImageSource, previousImageSourc);

		static async Task UpdateBitmap(
			this AImageView imageView,
			IImageElement newView,
			IImageElement previousView,
			ImageSource newImageSource,
			ImageSource previousImageSource)
		{

			IImageController imageController = newView as IImageController;
			newImageSource = newImageSource ?? newView?.Source;
			previousImageSource = previousImageSource ?? previousView?.Source;

			if (imageView.IsDisposed())
				return;

			if (newImageSource != null && Equals(previousImageSource, newImageSource))
				return;

			imageController?.SetIsLoading(true);

			(imageView as IImageRendererController)?.SkipInvalidate();
			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			try
			{
				if (newImageSource != null)
				{
					var imageViewHandler = Internals.Registrar.Registered.GetHandlerForObject<IImageViewHandler>(newImageSource);
					if (imageViewHandler != null)
					{
						await imageViewHandler.LoadImageAsync(newImageSource, imageView);
					}
					else
					{
						using (var drawable = await imageView.Context.GetFormsDrawableAsync(newImageSource))
						{
							// only set the image if we are still on the same one
							if (!imageView.IsDisposed() && Equals(newView?.Source, newImageSource))
								imageView.SetImageDrawable(drawable);
						}
					}
				}
				else
				{
					imageView.SetImageBitmap(null);
				}
			}
			finally
			{
				// only mark as finished if we are still working on the same image
				if (Equals(newView?.Source, newImageSource))
				{
					imageController?.SetIsLoading(false);
					imageController?.NativeSizeChanged();
				}
			}
		}
	}
}
