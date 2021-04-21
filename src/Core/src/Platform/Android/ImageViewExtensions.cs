using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class ImageViewExtensions
	{
		public static void UpdateAspect(this AppCompatImageView imageView, IImage image)
		{
			var scaleType = image.Aspect switch
			{
				Aspect.Fill => ImageView.ScaleType.FitXy,
				Aspect.AspectFill => ImageView.ScaleType.CenterCrop,
				Aspect.AspectFit => ImageView.ScaleType.FitCenter,
				_ => ImageView.ScaleType.FitCenter
			};
			
			imageView.SetScaleType(scaleType);
		}
	}
}