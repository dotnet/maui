#nullable disable
using System;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using GPaint = Microsoft.Maui.Graphics.Paint;
using Paint = Android.Graphics.Paint;

namespace Microsoft.Maui.Controls.Platform
{
	public static class BrushExtensions
	{
		public static void UpdateBackground(this AView view, Brush brush)
		{
			if (view is null)
				return;

			// Remove previous background gradient if any
			if (view.Background is GradientStrokeDrawable oldDrawable)
			{
				view.SetBackground(null);
				oldDrawable.Dispose();
			}

			// If brush is null, we must not set a background
			if (Brush.IsNullOrEmpty(brush))
			{
				return;
			}

			// Create a new gradient drawable and set it as the background
			var gradientStrokeDrawable = new GradientStrokeDrawable
			{
				Shape = new RectShape()
			};
			gradientStrokeDrawable.SetStroke(0, Colors.Transparent.ToPlatform());
			gradientStrokeDrawable.SetBrush(brush);
			view.Background = gradientStrokeDrawable;
		}

		public static void UpdateBackground(this Paint paint, Brush brush, int height, int width) =>
			((GPaint)brush).ApplyTo(paint, height, width);

		public static void UpdateBackground(this GradientDrawable gradientDrawable, Brush brush, int height, int width) =>
			((GPaint)brush).ApplyTo(gradientDrawable, height, width);

		public static bool UseGradients(this GradientDrawable gradientDrawable)
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(24))
				return false;

			var colors = gradientDrawable.GetColors();
			return colors != null && colors.Length > 1;
		}
	}
}