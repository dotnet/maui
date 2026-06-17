using Android.Content.Res;
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
			var thickness = picker.BorderThickness;
			var (nativeBackground, nativeBackgroundTintList) = GetNativeBackgroundState(platformView);

			// If thickness is zero, just tint underline (native Spinner background)
			if (thickness.IsEmpty)
			{
				platformView.Background = nativeBackground;
				platformView.BackgroundTintList = picker.BorderColor?.ToDefaultColorStateList() ?? nativeBackgroundTintList;
				return;
			}

			var backgroundTintList = picker.BorderColor?.ToDefaultColorStateList() ?? nativeBackgroundTintList;
			var color =
				picker.BorderColor?.ToPlatform()
				?? (backgroundTintList is not null ? new Color(backgroundTintList.DefaultColor) : (Color?)null)
				?? Colors.Black.ToPlatform();

			// Otherwise, draw a full border with per-side thickness
			platformView.BackgroundTintList = null;
			platformView.Background = new ThicknessDrawable(nativeBackground, nativeBackgroundTintList, backgroundTintList, thickness, color);
		}

		static (Drawable? Background, ColorStateList? TintList) GetNativeBackgroundState(AView platformView)
		{
			if (platformView.Background is ThicknessDrawable thicknessDrawable)
				return (thicknessDrawable.OriginalBackground, thicknessDrawable.OriginalBackgroundTintList);

			return (platformView.Background, platformView.BackgroundTintList);
		}

		private class ThicknessDrawable : Drawable
		{
			readonly Drawable? _backgroundDrawable;
			private readonly Thickness _thickness;
			private readonly Paint _paint;
			private readonly global::Android.Graphics.Rect _paddingRect;

			public ThicknessDrawable(Drawable? originalBackground, ColorStateList? originalBackgroundTintList, ColorStateList? backgroundTintList, Thickness thickness, Color color)
			{
				OriginalBackground = originalBackground;
				OriginalBackgroundTintList = originalBackgroundTintList;
				if (thickness.IsEmpty)
				{
					_backgroundDrawable = CloneDrawable(originalBackground);
					_backgroundDrawable?.SetTintList(backgroundTintList);
				}
				else
				{
					_backgroundDrawable = null;
				}
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

			public Drawable? OriginalBackground { get; }
			public ColorStateList? OriginalBackgroundTintList { get; }

			static Drawable? CloneDrawable(Drawable? drawable)
			{
				if (drawable is null)
					return null;

				return drawable.GetConstantState()?.NewDrawable()?.Mutate() ?? drawable.Mutate();
			}

			public override void Draw(Canvas canvas)
			{
				var w = Bounds.Width();
				var h = Bounds.Height();

				if (_backgroundDrawable is not null)
				{
					_backgroundDrawable.SetBounds(
						Bounds.Left,
						Bounds.Top,
						Bounds.Right,
						Bounds.Bottom
					);
					_backgroundDrawable.Draw(canvas);
				}

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
				var hasBackgroundPadding = _backgroundDrawable?.GetPadding(padding) ?? false;

				if (!hasBackgroundPadding)
					padding.SetEmpty();

				padding.Left += _paddingRect.Left;
				padding.Top += _paddingRect.Top;
				padding.Right += _paddingRect.Right;
				padding.Bottom += _paddingRect.Bottom;

				return hasBackgroundPadding || !_thickness.IsEmpty;
			}

			public override void SetAlpha(int alpha)
			{
				_paint.Alpha = alpha;
				_backgroundDrawable?.SetAlpha(alpha);
			}

			public override void SetColorFilter(ColorFilter? colorFilter)
			{
				_paint.SetColorFilter(colorFilter);
				_backgroundDrawable?.SetColorFilter(colorFilter);
			}
			public override int Opacity => (int)Format.Translucent;
		}

	}
}
