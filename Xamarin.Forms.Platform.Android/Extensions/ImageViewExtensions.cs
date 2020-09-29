using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Xamarin.Forms.Internals;
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
			imageView.Reset();
			imageView.SetImageResource(global::Android.Resource.Color.Transparent);

			try
			{
				if (newImageSource != null)
				{
					// all this animation code will go away if/once we pull in GlideX
					IFormsAnimationDrawable animation = null;

					if (imageController != null && imageController.GetLoadAsAnimation())
					{
						var animationHandler = Registrar.Registered.GetHandlerForObject<IAnimationSourceHandler>(newImageSource);
						if (animationHandler != null)
							animation = await animationHandler.LoadImageAnimationAsync(newImageSource, imageView.Context);
					}

					if (animation == null)
					{
						var imageViewHandler = Registrar.Registered.GetHandlerForObject<IImageViewHandler>(newImageSource);
						if (imageViewHandler != null)
						{
							await imageViewHandler.LoadImageAsync(newImageSource, imageView);
						}
						else
						{
							using (var drawable = await imageView.Context.GetFormsDrawableAsync(newImageSource))
							{
								// only set the image if we are still on the same one
								if (!imageView.IsDisposed() && SourceIsNotChanged(newView, newImageSource))
									imageView.SetImageDrawable(drawable);
							}
						}
					}
					else
					{
						if (!imageView.IsDisposed() && SourceIsNotChanged(newView, newImageSource))
							imageView.SetImageDrawable(animation.ImageDrawable);
						else
						{
							animation?.Reset();
							animation?.Dispose();
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
				if (SourceIsNotChanged(newView, newImageSource))
				{
					imageController?.SetIsLoading(false);
					imageController?.NativeSizeChanged();
				}
			}


			bool SourceIsNotChanged(IImageElement imageElement, ImageSource imageSource)
			{
				return (imageElement != null) ? imageElement.Source == imageSource : true;
			}
		}

		internal static void Reset(this IFormsAnimationDrawable formsAnimation)
		{
			if (formsAnimation is FormsAnimationDrawable animation)
			{
				if (!animation.IsDisposed())
				{
					animation.Stop();
					int frameCount = animation.NumberOfFrames;
					for (int i = 0; i < frameCount; i++)
					{
						var currentFrame = animation.GetFrame(i);
						if (currentFrame is BitmapDrawable bitmapDrawable)
						{
							var bitmap = bitmapDrawable.Bitmap;
							if (bitmap != null)
							{
								if (!bitmap.IsRecycled)
								{
									bitmap.Recycle();
								}
								bitmap.Dispose();
								bitmap = null;
							}
							bitmapDrawable.Dispose();
							bitmapDrawable = null;
						}
						currentFrame = null;
					}
					animation = null;
				}
			}
		}

		public static void Reset(this AImageView imageView)
		{
			if (!imageView.IsDisposed())
			{
				if (imageView.Drawable is FormsAnimationDrawable animation)
				{
					imageView.SetImageDrawable(null);
					animation.Reset();
				}

				imageView.SetImageResource(global::Android.Resource.Color.Transparent);
			}
		}

		internal static async void SetImage(this AImageView image, ImageSource source, Context context)
		{
			image.SetImageDrawable(await context.GetFormsDrawableAsync(source));
		}
	}
}