using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Microsoft.Maui.Graphics;
using static Android.Graphics.Drawables.GradientDrawable;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using Paint = Android.Graphics.Paint;

namespace Microsoft.Maui.Controls.Platform
{
	public static class BrushExtensions
	{
		public static void UpdateBackground(this AView view, Brush brush)
		{
			if (view == null)
				return;

			if (view.Background is GradientStrokeDrawable)
			{
				// Remove previous background gradient if any
				view.SetBackground(null);
			}

			if (Brush.IsNullOrEmpty(brush))
				return;

			if (brush is LinearGradientBrush linearGradientBrush)
			{
				GradientStopCollection gradients = linearGradientBrush.GradientStops;

				if (!IsValidGradient(gradients))
					return;
			}

			if (brush is RadialGradientBrush radialGradientBrush)
			{
				GradientStopCollection gradients = radialGradientBrush.GradientStops;

				if (!IsValidGradient(gradients))
					return;
			}

			view.SetPaintGradient(brush);
		}

		public static void UpdateBackground(this Paint paint, Brush brush, int height, int width)
		{
			if (paint == null || brush == null || brush.IsEmpty)
				return;

			if (brush is SolidColorBrush solidColorBrush)
			{
				var backgroundColor = solidColorBrush.Color;
				paint.Color = backgroundColor.ToNative();
			}

			if (brush is LinearGradientBrush linearGradientBrush)
			{
				var p1 = linearGradientBrush.StartPoint;
				var x1 = (float)p1.X;
				var y1 = (float)p1.Y;

				var p2 = linearGradientBrush.EndPoint;
				var x2 = (float)p2.X;
				var y2 = (float)p2.Y;

				var gradientBrushData = linearGradientBrush.GetGradientBrushData();
				var colors = gradientBrushData.Item1;
				var offsets = gradientBrushData.Item2;

				if (colors.Length < 2)
					return;

				var linearGradientShader = new LinearGradient(
					width * x1,
					height * y1,
					width * x2,
					height * y2,
					colors,
					offsets,
					Shader.TileMode.Clamp);

				paint.SetShader(linearGradientShader);
			}

			if (brush is RadialGradientBrush radialGradientBrush)
			{
				var center = radialGradientBrush.Center;
				float centerX = (float)center.X;
				float centerY = (float)center.Y;
				float radius = (float)radialGradientBrush.Radius;

				var gradientBrushData = radialGradientBrush.GetGradientBrushData();
				var colors = gradientBrushData.Item1;
				var offsets = gradientBrushData.Item2;

				if (colors.Length < 2)
					return;

				var radialGradientShader = new RadialGradient(
					width * centerX,
					height * centerY,
					Math.Max(height, width) * radius,
					colors,
					offsets,
					Shader.TileMode.Clamp);

				paint.SetShader(radialGradientShader);
			}
		}

		public static void UpdateBackground(this GradientDrawable gradientDrawable, Brush brush, int height, int width)
		{
			if (gradientDrawable == null || brush == null || brush.IsEmpty)
				return;

			if (brush is SolidColorBrush solidColorBrush)
			{
				Color bgColor = solidColorBrush.Color;
				gradientDrawable.SetColor(bgColor?.ToNative() ?? Colors.Transparent.ToNative());
			}

			if (brush is LinearGradientBrush linearGradientBrush)
			{
				var p1 = linearGradientBrush.StartPoint;
				var x1 = (float)p1.X;
				var y1 = (float)p1.Y;

				var p2 = linearGradientBrush.EndPoint;
				var x2 = (float)p2.X;
				var y2 = (float)p2.Y;

				const double Rad2Deg = 180.0 / Math.PI;

				float xDiff = x2 - x1;
				float yDiff = y2 - y1;

				double angle = Math.Atan2(yDiff, xDiff) * Rad2Deg;

				if (angle < 0)
					angle += 360;

				var gradientBrushData = linearGradientBrush.GetGradientBrushData();
				var colors = gradientBrushData.Item1;

				if (colors.Length < 2)
					return;

				gradientDrawable.SetGradientType(GradientType.LinearGradient);
				gradientDrawable.SetColors(colors);
				SetGradientOrientation(gradientDrawable, angle);
			}

			if (brush is RadialGradientBrush radialGradientBrush)
			{
				var center = radialGradientBrush.Center;
				float centerX = (float)center.X;
				float centerY = (float)center.Y;
				float radius = (float)radialGradientBrush.Radius;

				var gradientBrushData = radialGradientBrush.GetGradientBrushData();
				var colors = gradientBrushData.Item1;

				if (colors.Length < 2)
					return;

				gradientDrawable.SetGradientType(GradientType.RadialGradient);
				gradientDrawable.SetGradientCenter(centerX, centerY);
				gradientDrawable.SetGradientRadius(Math.Max(height, width) * radius);
				gradientDrawable.SetColors(colors);
			}
		}

		public static bool UseGradients(this GradientDrawable gradientDrawable)
		{
			if (!NativeVersion.IsAtLeast(24))
				return false;

			var colors = gradientDrawable.GetColors();
			return colors != null && colors.Length > 1;
		}

		internal static bool IsValidGradient(GradientStopCollection gradients)
		{
			if (gradients == null || gradients.Count == 0)
				return false;

			return true;
		}

		internal static void SetPaintGradient(this AView view, Brush brush)
		{
			var gradientStrokeDrawable = new GradientStrokeDrawable
			{
				Shape = new RectShape()
			};

			gradientStrokeDrawable.SetStroke(0, Colors.Transparent.ToNative());

			if (brush is SolidColorBrush solidColorBrush)
			{
				var color = solidColorBrush.Color?.ToNative() ?? Colors.Transparent.ToNative();
				gradientStrokeDrawable.SetColor(color);
			}
			else
				gradientStrokeDrawable.SetGradient(brush);

			view.Background?.Dispose();
			view.Background = gradientStrokeDrawable;
		}

		internal static void SetGradientOrientation(this GradientDrawable drawable, double angle)
		{
			Orientation orientation = Orientation.LeftRight;

			switch (angle)
			{
				case 0:
					orientation = Orientation.LeftRight;
					break;
				case 45:
					orientation = Orientation.TlBr;
					break;
				case 90:
					orientation = Orientation.TopBottom;
					break;
				case 135:
					orientation = Orientation.TrBl;
					break;
				case 180:
					orientation = Orientation.RightLeft;
					break;
				case 225:
					orientation = Orientation.BrTl;
					break;
				case 270:
					orientation = Orientation.BottomTop;
					break;
				case 315:
					orientation = Orientation.BlTr;
					break;
			}

			drawable.SetOrientation(orientation);
		}

		internal static Tuple<int[], float[]> GetGradientBrushData(this GradientBrush gradientBrush)
		{
			var orderStops = gradientBrush.GradientStops;

			int[] colors = new int[orderStops.Count];
			float[] offsets = new float[orderStops.Count];

			int count = 0;
			foreach (var orderStop in orderStops)
			{
				colors[count] = orderStop.Color.ToNative().ToArgb();
				offsets[count] = orderStop.Offset;
				count++;
			}

			return Tuple.Create(colors, offsets);
		}
	}
}