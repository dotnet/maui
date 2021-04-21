using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class ImageViewExtensions
	{
		static ImageView.ScaleType? s_fill;
		static ImageView.ScaleType? s_aspectFill;
		static ImageView.ScaleType? s_aspectFit;
		
		public static void UpdateAspect(this AppCompatImageView imageView, IImage image)
		{
			var scaleType = image.Aspect switch
			{
				Aspect.Fill => s_fill ??= ImageView.ScaleType.FitXy,
				Aspect.AspectFill => s_aspectFill ??= ImageView.ScaleType.CenterCrop,
				Aspect.AspectFit => s_aspectFit ??= ImageView.ScaleType.FitCenter,
				_ => s_aspectFit ??= ImageView.ScaleType.FitCenter
			};
			
			imageView.SetScaleType(scaleType);
		}
	}
}