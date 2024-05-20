using System;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AOrientation = Android.Graphics.Drawables.GradientDrawable.Orientation;
using APaint = Android.Graphics.Paint;

namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static Drawable? ToDrawable(this Paint? paint, Context? context)
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

		internal static bool IsValid(this GradientPaint? gradientPaint) =>
			gradientPaint?.GradientStops?.Length > 0;

		internal static GradientData GetGradientData(this GradientPaint gradientPaint, float? alphaOverride)
		{
			var orderStops = gradientPaint.GradientStops;

			var data = new GradientData(orderStops.Length);

			int count = 0;
			foreach (var orderStop in orderStops.OrderBy(s => s.Offset))
			{
				var color = orderStop.Color ?? Colors.Transparent;
				if (alphaOverride.HasValue)
					color = color.WithAlpha(alphaOverride.Value);

				data.Colors[count] = color.ToPlatform().ToArgb();
				data.Offsets[count] = orderStop.Offset;
				count++;
			}

			return data;
		}

		internal static LinearGradientData GetGradientData(this LinearGradientPaint linearGradientPaint, float? alphaOverride)
		{
			var p1 = linearGradientPaint.StartPoint;
			var x1 = (float)p1.X;
			var y1 = (float)p1.Y;

			var p2 = linearGradientPaint.EndPoint;
			var x2 = (float)p2.X;
			var y2 = (float)p2.Y;

			var data = ((GradientPaint)linearGradientPaint).GetGradientData(alphaOverride);
			var linearData = new LinearGradientData(data, x1, y1, x2, y2);

			return linearData;
		}

		internal static RadialGradientData GetGradientData(this RadialGradientPaint radialGradientPaint, float? alphaOverride)
		{
			var center = radialGradientPaint.Center;
			float centerX = (float)center.X;
			float centerY = (float)center.Y;

			float radius = (float)radialGradientPaint.Radius;

			var data = ((GradientPaint)radialGradientPaint).GetGradientData(alphaOverride);
			var radialData = new RadialGradientData(data.Colors, data.Offsets, centerX, centerY, radius);

			return radialData;
		}

		internal static LinearGradientShaderFactory GetGradientShaderFactory(this LinearGradientPaint linearGradientPaint, float? alphaOverride) =>
			new LinearGradientShaderFactory(linearGradientPaint.GetGradientData(alphaOverride));

		internal static RadialGradientShaderFactory GetGradientShaderFactory(this RadialGradientPaint radialGradientPaint, float? alphaOverride) =>
			new RadialGradientShaderFactory(radialGradientPaint.GetGradientData(alphaOverride));

		internal static void ApplyTo(this Paint? paint, APaint? nativePaint, int height, int width)
		{
			if (paint.IsNullOrEmpty() || nativePaint is null)
				return;

			if (paint is SolidPaint solidColorPaint)
			{
				var bgColor = solidColorPaint.Color ?? Colors.Transparent;
				nativePaint.Color = bgColor.ToPlatform();
			}
			else if (paint is LinearGradientPaint linearGradientPaint)
			{
				var data = linearGradientPaint.GetGradientData(null);
				var shader = data.ToShader(width, height);
				nativePaint.SetShader(shader);
			}
			else if (paint is RadialGradientPaint radialGradientPaint)
			{
				var data = radialGradientPaint.GetGradientData(null);
				var shader = data.ToShader(width, height);
				nativePaint.SetShader(shader);
			}
		}

		internal static void ApplyTo(this Paint paint, GradientDrawable gradientDrawable, int height, int width)
		{
			if (paint.IsNullOrEmpty() || gradientDrawable is null)
				return;

			if (paint is SolidPaint solidColorBrush)
			{
				var bgColor = solidColorBrush.Color ?? Colors.Transparent;
				gradientDrawable.SetColor(bgColor.ToPlatform());
			}
			else if (paint is LinearGradientPaint linearGradientBrush)
			{
				var data = linearGradientBrush.GetGradientData(null);
				var angle = data.GetGradientNearestAngle();
				gradientDrawable.SetGradientType(GradientType.LinearGradient);
				gradientDrawable.SetGradientOrientation(angle);
				if (OperatingSystem.IsAndroidVersionAtLeast(29))
					gradientDrawable.SetColors(data.Colors, data.Offsets);
				else
					gradientDrawable.SetColors(data.Colors);
			}
			else if (paint is RadialGradientPaint radialGradientBrush)
			{
				var data = radialGradientBrush.GetGradientData(null);
				gradientDrawable.SetGradientType(GradientType.RadialGradient);
				gradientDrawable.SetGradientCenter(data.CenterX, data.CenterY);
				gradientDrawable.SetGradientRadius(Math.Max(height, width) * data.Radius);
				if (OperatingSystem.IsAndroidVersionAtLeast(29))
					gradientDrawable.SetColors(data.Colors, data.Offsets);
				else
					gradientDrawable.SetColors(data.Colors);
			}
		}

		private static double GetGradientNearestAngle(this LinearGradientData data)
		{
			const double Rad2Deg = 180.0 / Math.PI;

			float xDiff = data.X2 - data.X1;
			float yDiff = data.Y2 - data.Y1;

			double angle = Math.Atan2(yDiff, xDiff) * Rad2Deg;

			if (angle < 0)
				angle += 360;

			return angle;
		}

		internal static void SetGradientOrientation(this GradientDrawable drawable, double angle)
		{
			AOrientation? orientation = AOrientation.LeftRight;

			switch (angle)
			{
				case 0:
					orientation = AOrientation.LeftRight;
					break;
				case 45:
					orientation = AOrientation.TlBr;
					break;
				case 90:
					orientation = AOrientation.TopBottom;
					break;
				case 135:
					orientation = AOrientation.TrBl;
					break;
				case 180:
					orientation = AOrientation.RightLeft;
					break;
				case 225:
					orientation = AOrientation.BrTl;
					break;
				case 270:
					orientation = AOrientation.BottomTop;
					break;
				case 315:
					orientation = AOrientation.BlTr;
					break;
			}

			drawable.SetOrientation(orientation);
		}
	}
}