using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static Drawable? ToDrawable(this Paint paint, Context? context)
		{
			if (paint is SolidPaint solidPaint)
				return solidPaint.CreateDrawable(context);

			if (paint is LinearGradientPaint linearGradientPaint)
				return linearGradientPaint.CreateDrawable(context);

			if (paint is RadialGradientPaint radialGradientPaint)
				return radialGradientPaint.CreateDrawable(context);

			if (paint is ImagePaint imagePaint)
				return imagePaint.CreateDrawable(context);

			if (paint is PatternPaint patternPaint)
				return patternPaint.CreateDrawable(context);

			return null;
		}

		public static Drawable? CreateDrawable(this SolidPaint solidPaint, Context? context)
		{
			var drawable = new MauiDrawable(context);
			drawable.SetBackground(solidPaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this LinearGradientPaint linearGradientPaint, Context? context)
		{
			if (!linearGradientPaint.IsValid())
				return null;

			var drawable = new MauiDrawable(context);
			drawable.SetBackground(linearGradientPaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this RadialGradientPaint radialGradientPaint, Context? context)
		{
			if (!radialGradientPaint.IsValid())
				return null;

			var drawable = new MauiDrawable(context);
			drawable.SetBackground(radialGradientPaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this ImagePaint imagePaint, Context? context)
		{
			var drawable = new MauiDrawable(context);
			drawable.SetBackground(imagePaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this PatternPaint patternPaint, Context? context)
		{
			var drawable = new MauiDrawable(context);
			drawable.SetBackground(patternPaint);

			return drawable;
		}

		static bool IsValid(this GradientPaint? gradienPaint) =>
			gradienPaint?.GradientStops?.Length > 0;
	}
}