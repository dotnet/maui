using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class ForegroundExtensions
	{
		public static void SetForeground(this TextView nativeView, IBrush? brush, Graphics.Color? defaultTextColor = null)
		{
			if (brush.IsNullOrEmpty())
				return;

			if (brush is ISolidColorBrush solidColorBrush)
			{
				var textColor = solidColorBrush.Color;

				if (textColor == null)
				{
					if (defaultTextColor != null)
						nativeView.SetTextColor(defaultTextColor.ToNative());
				}
				else
					nativeView.SetTextColor(textColor.ToNative());
			}

			if (brush is IGradientBrush gradientBrush)
			{
				var width = nativeView.Paint?.MeasureText(nativeView.Text?.ToString()) ?? 0;
				var height = nativeView.TextSize;

				if (height > 0 && width > 0)
				{
					var textShader = gradientBrush.ToShader(height, width);
					nativeView.Paint?.SetShader(textShader);
				}
			}
		}

		public static void SetForeground(this TextView nativeView, IBrush? brush, ColorStateList? defaultTextColor)
		{
			if (brush.IsNullOrEmpty())
				return;

			if (brush is ISolidColorBrush solidColorBrush)
			{
				var textColor = solidColorBrush.Color;

				if (textColor == null)
					nativeView.SetTextColor(defaultTextColor);
				else
					nativeView.SetTextColor(textColor.ToNative());
			}

			if (brush is IGradientBrush gradientBrush)
			{
				var width = nativeView.Paint?.MeasureText(nativeView.Text?.ToString()) ?? 0;
				var height = nativeView.TextSize;

				if (height > 0 && width > 0)
				{
					var textShader = gradientBrush.ToShader(height, width);
					nativeView.Paint?.SetShader(textShader);
				}
			}
		}

		public static Shader? ToShader(this IBrush brush, float height, float width)
		{
			if (width == 0 && height == 0)
				return null;

			if (brush is ILinearGradientBrush linearGradientBrush)
			{
				var p1 = linearGradientBrush.StartPoint;
				var x1 = (float)p1.X;
				var y1 = (float)p1.Y;

				var p2 = linearGradientBrush.EndPoint;
				var x2 = (float)p2.X;
				var y2 = (float)p2.Y;

				var orderStops = linearGradientBrush.GradientStops;

				var colors = new int[orderStops.Count];
				var offsets = new float[orderStops.Count];

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

			if (brush is IRadialGradientBrush radialGradientBrush)
			{
				var center = radialGradientBrush.Center;
				float centerX = (float)center.X;
				float centerY = (float)center.Y;
				float radius = (float)radialGradientBrush.Radius;

				var orderStops = radialGradientBrush.GradientStops;

				var colors = new int[orderStops.Count];
				var offsets = new float[orderStops.Count];

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