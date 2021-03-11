using System;
using Android.Graphics.Drawables.Shapes;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class BrushExtensions
	{
		public static void UpdateBackground(this AView view, IBrush brush)
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

		internal static bool IsValidGradient(GradientStopCollection gradients)
		{
			if (gradients == null || gradients.Count == 0)
				return false;

			return true;
		}

		internal static void SetPaintGradient(this AView view, IBrush brush)
		{
			var gradientStrokeDrawable = new GradientStrokeDrawable
			{
				Shape = new RectShape()
			};

			gradientStrokeDrawable.SetStroke(0, Color.Default.ToNative());

			if (brush is SolidColorBrush solidColorBrush)
			{
				var color = solidColorBrush.Color.IsDefault ? Color.Default.ToNative() : solidColorBrush.Color.ToNative();
				gradientStrokeDrawable.SetColor(color);
			}
			else
				gradientStrokeDrawable.SetGradient(brush);

			view.Background?.Dispose();
			view.Background = gradientStrokeDrawable;
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