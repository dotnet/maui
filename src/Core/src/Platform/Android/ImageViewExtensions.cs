using Android.Graphics.Drawables;
using Android.Widget;
using Google.Android.Material.ImageView;

namespace Microsoft.Maui.Platform
{
	public static class ImageViewExtensions
	{
		public static void Clear(this ImageView imageView)
		{
			// stop the animation
			if (imageView.Drawable is IAnimatable animatable)
				animatable.Stop();

			// clear the view and release any bitmaps
			imageView.SetImageResource(global::Android.Resource.Color.Transparent);
		}

		public static void UpdateAspect(this ImageView imageView, IImage image)
		{
			if (image.Aspect == Aspect.AspectFill)
			{
				imageView.SetAdjustViewBounds(false);
			}
			else
			{
				imageView.SetAdjustViewBounds(true);
			}

			imageView.SetScaleType(image.Aspect.ToScaleType());
		}

		public static void UpdateIsAnimationPlaying(this ImageView imageView, IImageSourcePart image) =>
			imageView.Drawable.UpdateIsAnimationPlaying(image);

		public static void UpdateIsAnimationPlaying(this Drawable? drawable, IImageSourcePart image)
		{
			if (drawable.IsAlive() && drawable.AsAnimatable() is IAnimatable animatable)
			{
				if (image.IsAnimationPlaying)
				{
					if (!animatable.IsRunning)
						animatable.Start();
				}
				else
				{
					if (animatable.IsRunning)
						animatable.Stop();
				}
			}
		}
	}
}