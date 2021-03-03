using Android.Widget;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class ImageExtensions
	{
		static ImageView.ScaleType s_fill;
		static ImageView.ScaleType s_aspectFill;
		static ImageView.ScaleType s_aspectFit;

		public static ImageView.ScaleType ToScaleType(this Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.Fill:
					return s_fill ?? (s_fill = ImageView.ScaleType.FitXy);
				case Aspect.AspectFill:
					return s_aspectFill ?? (s_aspectFill = ImageView.ScaleType.CenterCrop);
				default:
				case Aspect.AspectFit:
					return s_aspectFit ?? (s_aspectFit = ImageView.ScaleType.FitCenter);
			}
		}
	}
}