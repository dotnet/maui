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

		internal static GradientData GetGradientPaintData(GradientPaint gradientPaint)
		{
			var orderStops = gradientPaint.GradientStops;

			var data = new GradientData(orderStops.Length);

			int count = 0;
			foreach (var orderStop in orderStops)
			{
				data.Colors[count] = orderStop.Color.ToNative().ToArgb();
				data.Offsets[count] = orderStop.Offset;
				count++;
			}

			return data;
		}

		internal static LinearGradientShaderFactory GetLinearGradientShaderFactory(LinearGradientPaint linearGradientPaint)
		{
			var p1 = linearGradientPaint.StartPoint;
			var x1 = (float)p1.X;
			var y1 = (float)p1.Y;

			var p2 = linearGradientPaint.EndPoint;
			var x2 = (float)p2.X;
			var y2 = (float)p2.Y;
			var data = GetGradientPaintData(linearGradientPaint);
			var linearGradientShaderFactory = new LinearGradientShaderFactory(new LinearGradientData(data.Colors, data.Offsets, x1, y1, x2, y2));

			return linearGradientShaderFactory;
		}

		internal static RadialGradientShaderFactory GetRadialGradientShaderFactory(RadialGradientPaint radialGradientPaint)
		{
			var center = radialGradientPaint.Center;
			float centerX = (float)center.X;
			float centerY = (float)center.Y;
			float radius = (float)radialGradientPaint.Radius;

			var data = PaintExtensions.GetGradientPaintData(radialGradientPaint);
			var shader = new RadialGradientData(data.Colors, data.Offsets, centerX, centerY, radius);

			var radialGradientShaderFactory = new RadialGradientShaderFactory(shader);
			return radialGradientShaderFactory;
		}
	}
}