using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class MauiCALayer : CALayer
    {
        CGRect _bounds;

        CornerRadius _cornerRadius;

        UIColor? _backgroundColor;
        Paint? _background;

        float _borderWidth;
        UIColor? _borderColor;
        Paint? _border;

        public MauiCALayer()
        {
            _bounds = new CGRect();

            ContentsScale = UIScreen.MainScreen.Scale;
        }

        public override void LayoutSublayers()
        {
            base.LayoutSublayers();

            if (Bounds.Equals(_bounds))
                return;

            _bounds = new CGRect(Bounds.Location, Bounds.Size);
        }

        public override void DrawInContext(CGContext ctx)
        {
            base.DrawInContext(ctx);

            ctx.AddPath(GetClipPath());
            ctx.Clip();

            DrawBackground(ctx);
            DrawBorder(ctx);
        }

        public void SetCornerRadius(CornerRadius cornerRadius)
        {
            _cornerRadius = cornerRadius;

            SetNeedsDisplay();
        }

        public void SetBackground(Paint? paint)
        {
            if (paint is SolidPaint solidPaint)
                SetBackground(solidPaint);

            if (paint is LinearGradientPaint linearGradientPaint)
                SetBackground(linearGradientPaint);

            if (paint is RadialGradientPaint radialGradientPaint)
                SetBackground(radialGradientPaint);

            if (paint is ImagePaint imagePaint)
                SetBackground(imagePaint);

            if (paint is PatternPaint patternPaint)
                SetBackground(patternPaint);
        }

        public void SetBackground(SolidPaint solidPaint)
        {
            _backgroundColor = solidPaint.Color == null
                ? (UIColor?)null
                : solidPaint.Color.ToNative();
            
            _background = null;

            SetNeedsDisplay();
        }

        public void SetBackground(LinearGradientPaint linearGradientPaint)
        {
            _backgroundColor = null;
            _background = linearGradientPaint;

            SetNeedsDisplay();
        }

        public void SetBackground(RadialGradientPaint radialGradientPaint)
        {
            _backgroundColor = null;
            _background = radialGradientPaint;

            SetNeedsDisplay();
        }

        public void SetBackground(ImagePaint imagePaint)
        {
            throw new NotImplementedException();
        }

        public void SetBackground(PatternPaint patternPaint)
        {
            throw new NotImplementedException();
        }

        public void SetBorderBrush(Paint? paint)
        {
            if (paint is SolidPaint solidPaint)
                SetBorderBrush(solidPaint);

            if (paint is LinearGradientPaint linearGradientPaint)
                SetBorderBrush(linearGradientPaint);

            if (paint is RadialGradientPaint radialGradientPaint)
                SetBorderBrush(radialGradientPaint);

            if (paint is ImagePaint imagePaint)
                SetBorderBrush(imagePaint);

            if (paint is PatternPaint patternPaint)
                SetBorderBrush(patternPaint);
        }

        public void SetBorderBrush(SolidPaint solidPaint)
        {
            _borderColor = solidPaint.Color == null
                ? (UIColor?)null
                : solidPaint.Color.ToNative();

            _border = null;

            SetNeedsDisplay();
        }

        public void SetBorderBrush(LinearGradientPaint linearGradientPaint)
        {
            _borderColor = null;
            _border = linearGradientPaint;

            SetNeedsDisplay();
        }

        public void SetBorderBrush(RadialGradientPaint radialGradientPaint)
        {
            _borderColor = null;
            _border = radialGradientPaint;

            SetNeedsDisplay();
        }

        public void SetBorderBrush(ImagePaint imagePaint)
        {
            throw new NotImplementedException();
        }

        public void SetBorderBrush(PatternPaint patternPaint)
        {
            throw new NotImplementedException();
        }

        public void SetBorderWidth(double borderWidth)
        {
            _borderWidth = (float)borderWidth;

            SetNeedsDisplay();
        }

        CGPath? GetClipPath() =>
            GetRoundCornersPath(Bounds, _cornerRadius, _borderWidth).CGPath;

        void DrawBackground(CGContext ctx)
        {
            if (_background != null)
                DrawGradientPaint(ctx, _background);
            else if (_backgroundColor != null)
            {
                ctx.SetFillColor(_backgroundColor.CGColor);
                ctx.AddPath(GetClipPath());
                ctx.DrawPath(CGPathDrawingMode.Fill);
            }
        }

        void DrawBorder(CGContext ctx)
        {
            if (_borderWidth == 0)
                return;

            ctx.SetLineWidth(2 * _borderWidth);
            ctx.AddPath(GetClipPath());

            if (_border != null)
            {
                ctx.ReplacePathWithStrokedPath();
                ctx.Clip();

                DrawGradientPaint(ctx, _border);
            }
            else if (_borderColor != null)
            {
                ctx.SetStrokeColor(_borderColor.CGColor);
                ctx.DrawPath(CGPathDrawingMode.Stroke);
            }
        }

        void DrawGradientPaint(CGContext graphics, Paint paint)
        {
            if (paint == null)
                return;

            if (paint is GradientPaint gradientPaint)
            {
                using (CGColorSpace rgb = CGColorSpace.CreateDeviceRGB())
                {
                    CGColor[] colors = new CGColor[gradientPaint.GradientStops.Length];
                    nfloat[] locations = new nfloat[gradientPaint.GradientStops.Length];

                    for (int index = 0; index < gradientPaint.GradientStops.Length; index++)
                    {
                        Color color = gradientPaint.GradientStops[index].Color;
                        colors[index] = new CGColor(new nfloat(color.Red), new nfloat(color.Green), new nfloat(color.Blue), new nfloat(color.Alpha));
                        locations[index] = new nfloat(gradientPaint.GradientStops[index].Offset);
                    }

                    CGGradient gradient = new CGGradient(rgb, colors, locations);

                    if (gradientPaint is LinearGradientPaint linearGradientPaint)
                    {
                        graphics.DrawLinearGradient(
                            gradient,
                            new CGPoint(_bounds.Left + linearGradientPaint.StartPoint.X * _bounds.Width, _bounds.Top + linearGradientPaint.StartPoint.Y * _bounds.Height),
                            new CGPoint(_bounds.Left + linearGradientPaint.EndPoint.X * _bounds.Width, _bounds.Top + linearGradientPaint.EndPoint.Y * _bounds.Height),
                            CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);
                    }

                    if (gradientPaint is RadialGradientPaint radialGradientPaint)
                    {
                        graphics.DrawRadialGradient(
                            gradient,
                            new CGPoint(radialGradientPaint.Center.X * _bounds.Width + _bounds.Left, radialGradientPaint.Center.Y * _bounds.Height + _bounds.Top),
                            0.0f,
                            new CGPoint(radialGradientPaint.Center.X * _bounds.Width + _bounds.Left, radialGradientPaint.Center.Y * _bounds.Height + _bounds.Top),
                            (nfloat)(radialGradientPaint.Radius * Math.Max(_bounds.Height, _bounds.Width)),
                            CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);
                    }
                }
            }
        }

        UIBezierPath GetRoundCornersPath(CGRect bounds, CornerRadius cornerRadius, float borderWidth = 0f)
        {
            if (cornerRadius == new CornerRadius(0d))
            {
                return UIBezierPath.FromRect(bounds);
            }

            var topLeft = ValidateCornerRadius(cornerRadius.TopLeft, borderWidth);
            var topRight = ValidateCornerRadius(cornerRadius.TopRight, borderWidth);
            var bottomLeft = ValidateCornerRadius(cornerRadius.BottomLeft, borderWidth);
            var bottomRight = ValidateCornerRadius(cornerRadius.BottomRight, borderWidth);

            var bezierPath = new UIBezierPath();
            bezierPath.AddArc(new CGPoint((float)bounds.X + bounds.Width - topRight, (float)bounds.Y + topRight), topRight, (float)(Math.PI * 1.5), (float)Math.PI * 2, true);
            bezierPath.AddArc(new CGPoint((float)bounds.X + bounds.Width - bottomRight, (float)bounds.Y + bounds.Height - bottomRight), bottomRight, 0, (float)(Math.PI * .5), true);
            bezierPath.AddArc(new CGPoint((float)bounds.X + bottomLeft, (float)bounds.Y + bounds.Height - bottomLeft), bottomLeft, (float)(Math.PI * .5), (float)Math.PI, true);
            bezierPath.AddArc(new CGPoint((float)bounds.X + topLeft, (float)bounds.Y + topLeft), topLeft, (float)Math.PI, (float)(Math.PI * 1.5), true);
            bezierPath.ClosePath();

            return bezierPath;
        }

        float ValidateCornerRadius(double corner, float borderWidth)
        {
            var cornerRadius = corner - borderWidth;
            return cornerRadius > 0 ? (float)cornerRadius : 0;
        }
    }
}