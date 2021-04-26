using Android.Widget;

namespace Microsoft.Maui
{
	public static class AspectExtensions
	{
		static ImageView.ScaleType? AspectFit;
		static ImageView.ScaleType? AspectFill;
		static ImageView.ScaleType? Fill;
		static ImageView.ScaleType? Center;

		public static ImageView.ScaleType ToScaleType(this Aspect aspect) =>
			aspect switch
			{
				Aspect.AspectFit => AspectFit ??= ImageView.ScaleType.FitCenter!,
				Aspect.AspectFill => AspectFill ??= ImageView.ScaleType.CenterCrop!,
				Aspect.Fill => Fill ??= ImageView.ScaleType.FitXy!,
				Aspect.Center => Center ??= ImageView.ScaleType.Center!,
				_ => AspectFit ??= ImageView.ScaleType.FitCenter!,
			};
	}
}