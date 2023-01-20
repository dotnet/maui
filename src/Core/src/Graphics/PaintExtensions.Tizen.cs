using TColor = Tizen.UIExtensions.Common.Color;

namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static TColor ToPlatform(this Paint paint)
		{
			var color = paint.ToColor();
			return color != null ? color.ToPlatform() : TColor.Default;
		}

		public static MauiDrawable? ToDrawable(this Paint paint)
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

		public static MauiDrawable? CreateDrawable(this SolidPaint solidPaint)
		{
			return new MauiDrawable
			{
				Background = solidPaint
			};
		}

		public static MauiDrawable? CreateDrawable(this LinearGradientPaint linearGradientPaint)
		{
			if (!linearGradientPaint.IsValid())
				return null;

			return new MauiDrawable
			{
				Background = linearGradientPaint
			};
		}

		public static MauiDrawable? CreateDrawable(this RadialGradientPaint radialGradientPaint)
		{
			if (!radialGradientPaint.IsValid())
				return null;

			return new MauiDrawable
			{
				Background = radialGradientPaint
			};
		}

		public static MauiDrawable? CreateDrawable(this ImagePaint imagePaint)
		{
			return new MauiDrawable
			{
				Background = imagePaint
			};
		}

		public static MauiDrawable? CreateDrawable(this PatternPaint patternPaint)
		{
			return new MauiDrawable
			{
				Background = patternPaint
			};
		}

		static bool IsValid(this GradientPaint? gradientPaint) =>
			gradientPaint?.GradientStops?.Length > 0;
	}
}