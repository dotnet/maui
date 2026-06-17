using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using APaint = Android.Graphics.Paint;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Controls.Platform
{
    public static class PickerExtensions
    {
        public static void CreateBorder(this AView platformView, Picker picker)
        {
            var thickness = picker.BorderThickness;
            var (nativeBackground, nativeBackgroundTintList) = GetNativeBackgroundState(platformView);

            // Scale Thickness (DIPs) to pixels
            var density = platformView.Context?.Resources?.DisplayMetrics?.Density ?? 1f;
            var pxThickness = new Thickness(
                thickness.Left * density,
                thickness.Top * density,
                thickness.Right * density,
                thickness.Bottom * density
            );

            // No thickness → restore native background + tint
            if (pxThickness.IsEmpty)
            {
                platformView.Background = nativeBackground;
                platformView.BackgroundTintList = picker.BorderColor?.ToDefaultColorStateList() ?? nativeBackgroundTintList;
                return;
            }

            // Thickness set, but no color → use visible default (Black) to match Windows
            var color = picker.BorderColor?.ToPlatform() ?? Colors.Black.ToPlatform();
            var backgroundTintList = picker.BorderColor?.ToDefaultColorStateList() ?? nativeBackgroundTintList;

            // Draw custom border on top of preserved native background
            platformView.BackgroundTintList = null;
            platformView.Background = new ThicknessDrawable(
                nativeBackground,
                nativeBackgroundTintList,
                backgroundTintList,
                pxThickness,
                color
            );
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
            readonly Thickness _thickness;
            readonly APaint _paint;
            readonly ARect _paddingRect;

            public ThicknessDrawable(
                Drawable? originalBackground,
                ColorStateList? originalBackgroundTintList,
                ColorStateList? backgroundTintList,
                Thickness thickness,
                AColor color)
            {
                OriginalBackground = originalBackground;
                OriginalBackgroundTintList = originalBackgroundTintList;

                // Preserve native background (caret, ripple, etc.)
                _backgroundDrawable = CloneDrawable(originalBackground);
                _backgroundDrawable?.SetTintList(originalBackgroundTintList ?? backgroundTintList);

                _thickness = thickness;

                _paint = new APaint
                {
                    Color = color,
                    AntiAlias = true,
                    StrokeWidth = 0,
                };
                _paint.SetStyle(APaint.Style.Fill);

                _paddingRect = new ARect(
                    (int)_thickness.Left,
                    (int)_thickness.Top,
                    (int)_thickness.Right,
                    (int)_thickness.Bottom
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

            public override bool GetPadding(ARect padding)
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
