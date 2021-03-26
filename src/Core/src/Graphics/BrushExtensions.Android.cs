using Android.Graphics.Drawables;

namespace Microsoft.Maui.Graphics
{
	public static partial class BrushExtensions
	{
		public static Drawable? CreateDrawable(this ISolidColorBrush solidColorBrush)
		{
			var drawable = new MauiDrawable();
			drawable.SetBrush(solidColorBrush);
			return drawable;
		}

		public static Drawable? CreateDrawable(this ILinearGradientBrush linearGradientBrush)
		{
			if (!linearGradientBrush.IsValid())
				return null;

			var drawable = new MauiDrawable();
			drawable.SetBrush(linearGradientBrush);
			return drawable;
		}

		public static Drawable? CreateDrawable(this IRadialGradientBrush radialGradientBrush)
		{
			if (!radialGradientBrush.IsValid())
				return null;

			var drawable = new MauiDrawable();
			drawable.SetBrush(radialGradientBrush);
			return drawable;
		}

		static bool IsValid(this IGradientBrush? gradientBrush) =>
			gradientBrush?.GradientStops?.Count > 0;
	}
}