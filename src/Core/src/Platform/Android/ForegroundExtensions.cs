using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Widget;
using Microsoft.Maui.Graphics;
using Color = Microsoft.Maui.Graphics.Color;
using Paint = Microsoft.Maui.Graphics.Paint;

namespace Microsoft.Maui
{
	public static class ForegroundExtensions
	{
		public static void SetForeground(this TextView nativeView, Paint? paint, Color? defaultTextColor = null)
		{
			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				var textColor = solidPaint.Color;

				if (textColor == null)
				{
					if (defaultTextColor != null)
						nativeView.SetTextColor(defaultTextColor.ToNative());
				}
				else
					nativeView.SetTextColor(textColor.ToNative());
			}

			if (paint is GradientPaint gradientPaint)
			{
				var width = nativeView.Paint?.MeasureText(nativeView.Text?.ToString()) ?? 0;
				var height = nativeView.TextSize;

				if (height > 0 && width > 0)
				{
					var textShader = gradientPaint.ToShader(height, width);
					nativeView.Paint?.SetShader(textShader);
				}
			}
		}

		public static void SetForeground(this TextView nativeView, Paint? paint, ColorStateList? defaultTextColor)
		{
			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				var textColor = solidPaint.Color;

				if (textColor == null)
					nativeView.SetTextColor(defaultTextColor);
				else
					nativeView.SetTextColor(textColor.ToNative());
			}

			if (paint is GradientPaint gradientPaint)
			{
				var width = nativeView.Paint?.MeasureText(nativeView.Text?.ToString()) ?? 0;
				var height = nativeView.TextSize;

				if (height > 0 && width > 0)
				{
					var textShader = gradientPaint.ToShader(height, width);
					nativeView.Paint?.SetShader(textShader);
				}
			}
		}

		public static Shader? ToShader(this Paint paint, float height, float width)
		{
			if (width == 0 && height == 0)
				return null;

			if (paint is LinearGradientPaint linearGradientPaint)
			{
				var p1 = linearGradientPaint.StartPoint;
				var x1 = (float)p1.X;
				var y1 = (float)p1.Y;

				var p2 = linearGradientPaint.EndPoint;
				var x2 = (float)p2.X;
				var y2 = (float)p2.Y;

				var orderStops = linearGradientPaint.GradientStops;

				var colors = new int[orderStops.Length];
				var offsets = new float[orderStops.Length];

				int count = 0;
				foreach (var orderStop in orderStops)
				{
					colors[count] = orderStop.Color.ToNative().ToArgb();
					offsets[count] = orderStop.Offset;
					count++;
				}

				if (colors == null || colors.Length < 2)
					return null;

				return new LinearGradient(width * x1, height * y1, width * x2, height * y2, colors, offsets, Shader.TileMode.Clamp!);
			}

			if (paint is RadialGradientPaint radialGradientPaint)
			{
				var center = radialGradientPaint.Center;
				float centerX = (float)center.X;
				float centerY = (float)center.Y;
				float radius = (float)radialGradientPaint.Radius;

				var orderStops = radialGradientPaint.GradientStops;

				var colors = new int[orderStops.Length];
				var offsets = new float[orderStops.Length];

				int count = 0;
				foreach (var orderStop in orderStops)
				{
					colors[count] = orderStop.Color.ToNative().ToArgb();
					offsets[count] = orderStop.Offset;
					count++;
				}

				if (colors == null || colors.Length < 2)
					return null;

				return new RadialGradient(width * centerX, height * centerY, Math.Max(height, width) * radius, colors, offsets, Shader.TileMode.Clamp!);
			}

			return null;
		}
	}
}