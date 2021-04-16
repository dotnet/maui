using Android.Widget;

namespace Microsoft.Maui
{
	public static class AspectExtensions
	{
		static ImageView.ScaleType? Fill;
		static ImageView.ScaleType? AspectFill;
		static ImageView.ScaleType? AspectFit;

		public static ImageView.ScaleType ToScaleType(this Aspect aspect) =>
			aspect switch
			{
				Aspect.Fill => Fill ??= ImageView.ScaleType.FitXy!,
				Aspect.AspectFill => AspectFill ??= ImageView.ScaleType.CenterCrop!,
				_ => AspectFit ??= ImageView.ScaleType.FitCenter!,
			};
	}
}