using Android.Graphics;
using Android.Graphics.Drawables;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using Color = Android.Graphics.Color;
using Paint = Android.Graphics.Paint;

namespace Microsoft.Maui.Controls.Platform
{
	public static class PickerExtensions
	{
		public static void CreateBorder(this AView platformView, Picker picker)
		{
			Color? color = null;
			color = picker.BorderColor != null ? picker.BorderColor.ToPlatform() : Colors.Transparent.ToPlatform();

			var thickness = picker.BorderThickness;

			// If thickness is zero, just tint underline (native Spinner background)
			if (thickness.IsEmpty)
			{
				platformView.BackgroundTintList = picker.BorderColor?.ToDefaultColorStateList();
				return;
			}

			// Otherwise, draw a full border with per-side thickness
			platformView.Background = new ThicknessDrawable(thickness, (Color)color);
		}
		private class ThicknessDrawable : Drawable
		{
			private readonly Thickness _thickness;
			private readonly Paint _paint;
			private readonly global::Android.Graphics.Rect _paddingRect;

			public ThicknessDrawable(Thickness thickness, Color color)
			{
				_thickness = thickness;
				_paint = new Paint
				{
					Color = color,
					AntiAlias = true,
					StrokeWidth = 0,
				};
				_paint.SetStyle(Paint.Style.Fill);
				_paddingRect = new global::Android.Graphics.Rect(
					(int)thickness.Left,
					(int)thickness.Top,
					(int)thickness.Right,
					(int)thickness.Bottom
				);
			}

			public override void Draw(Canvas canvas)
			{
				var w = Bounds.Width();
				var h = Bounds.Height();

				// Top
				if (_thickness.Top > 0)
				{
					canvas.DrawRect(0, 0, w, (float)_thickness.Top, _paint);
				}

				// Bottom
				if (_thickness.Bottom > 0)
				{
					canvas.DrawRect(0, h - (float)_thickness.Bottom, w, h, _paint);
				}

				// Left
				if (_thickness.Left > 0)
				{
					canvas.DrawRect(0, 0, (float)_thickness.Left, h, _paint);
				}

				// Right
				if (_thickness.Right > 0)
				{
					canvas.DrawRect(w - (float)_thickness.Right, 0, w, h, _paint);
				}
			}

			public override bool GetPadding(global::Android.Graphics.Rect padding)
			{
				padding.Set(_paddingRect);
				return true;
			}

			public override void SetAlpha(int alpha) => _paint.Alpha = alpha;
			public override void SetColorFilter(ColorFilter? colorFilter) => _paint.SetColorFilter(colorFilter);
			public override int Opacity => (int)Format.Translucent;
		}

	}
}
