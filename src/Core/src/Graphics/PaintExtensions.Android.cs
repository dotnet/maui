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

		static bool IsValid(this GradientPaint? gradientPaint) =>
			gradientPaint?.GradientStops?.Length > 0;

		internal static GradientData GetGradientPaintData(GradientPaint gradientPaint, float alpha = 1.0f)
		{
			var orderStops = gradientPaint.GradientStops;

			var data = new GradientData(orderStops.Length);

			int count = 0;
			foreach (var orderStop in orderStops)
			{
				data.Colors[count] = orderStop.Color.WithAlpha(alpha).ToPlatform().ToArgb();
				data.Offsets[count] = orderStop.Offset;
				count++;
			}

			return data;
		}

		internal static LinearGradientShaderFactory GetLinearGradientShaderFactory(LinearGradientPaint linearGradientPaint, float alpha = 1.0f)
		{
			var p1 = linearGradientPaint.StartPoint;
			var x1 = (float)p1.X;
			var y1 = (float)p1.Y;

			var p2 = linearGradientPaint.EndPoint;
			var x2 = (float)p2.X;
			var y2 = (float)p2.Y;
			var data = GetGradientPaintData(linearGradientPaint, alpha);
			var linearGradientShaderFactory = new LinearGradientShaderFactory(new LinearGradientData(data.Colors, data.Offsets, x1, y1, x2, y2));

			return linearGradientShaderFactory;
		}

		internal static RadialGradientShaderFactory GetRadialGradientShaderFactory(RadialGradientPaint radialGradientPaint, float alpha = 1.0f)
		{
			var center = radialGradientPaint.Center;
			float centerX = (float)center.X;
			float centerY = (float)center.Y;
			float radius = (float)radialGradientPaint.Radius;

			var data = GetGradientPaintData(radialGradientPaint, alpha);
			var shader = new RadialGradientData(data.Colors, data.Offsets, centerX, centerY, radius);

			var radialGradientShaderFactory = new RadialGradientShaderFactory(shader);
			return radialGradientShaderFactory;
		}
	}
}