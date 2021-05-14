using Android.Graphics.Drawables;

namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static Drawable? ToDrawable(this Paint paint)
		{
			if (paint is SolidPaint solidPaint)
				return solidPaint.CreateDrawable();

			if (paint is LinearGradientPaint linearGradientPaint)
				return linearGradientPaint.CreateDrawable();

			if (paint is RadialGradientPaint radialGradientPaint)
				return radialGradientPaint.CreateDrawable();

			if (paint is ImagePaint imagePaint)
				return imagePaint.CreateDrawable();

			if (paint is PatternPaint patternPaint)
				return patternPaint.CreateDrawable();

			return null;
		}

		public static Drawable? CreateDrawable(this SolidPaint solidPaint)
		{
			var drawable = new MauiDrawable();
			drawable.SetPaint(solidPaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this LinearGradientPaint linearGradientPaint)
		{
			if (!linearGradientPaint.IsValid())
				return null;

			var drawable = new MauiDrawable();
			drawable.SetPaint(linearGradientPaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this RadialGradientPaint radialGradientPaint)
		{
			if (!radialGradientPaint.IsValid())
				return null;

			var drawable = new MauiDrawable();
			drawable.SetPaint(radialGradientPaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this ImagePaint imagePaint)
		{
			var drawable = new MauiDrawable();
			drawable.SetPaint(imagePaint);

			return drawable;
		}

		public static Drawable? CreateDrawable(this PatternPaint patternPaint)
		{
			var drawable = new MauiDrawable();
			drawable.SetPaint(patternPaint);

			return drawable;
		}

		static bool IsValid(this GradientPaint? gradienPaint) =>
			gradienPaint?.GradientStops?.Length > 0;
	}
}