using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui
{
	public class MauiCALayer : CALayer
	{
		CGRect _bounds;

		IShape? _shape;

		UIColor? _backgroundColor;
		Paint? _background;

		float _borderWidth;
		UIColor? _borderColor;
		Paint? _border;

		nfloat[]? _borderDash;
		nfloat _borderDashOffset;

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

			var clipPath = GetClipPath();

			if (clipPath != null)
				ctx.AddPath(clipPath);

			ctx.Clip();

			DrawBackground(ctx);
			DrawBorder(ctx);
		}

		public void SetBorderShape(IShape shape)
		{
			_shape = shape;

			SetNeedsDisplay();
		}

		public void SetBackground(Paint? paint)
		{
			if (paint is SolidPaint solidPaint)
				SetBackground(solidPaint);
			else if (paint is LinearGradientPaint linearGradientPaint)
				SetBackground(linearGradientPaint);
			else if (paint is RadialGradientPaint radialGradientPaint)
				SetBackground(radialGradientPaint);
			else if (paint is ImagePaint imagePaint)
				SetBackground(imagePaint);
			else if (paint is PatternPaint patternPaint)
				SetBackground(patternPaint);
			else
				SetDefaultBackgroundColor();
		}

		public void SetBackground(SolidPaint solidPaint)
		{
			if (solidPaint.Color == null)
				SetDefaultBackgroundColor();
			else
				_backgroundColor = solidPaint.Color.ToNative();

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

		public void SetBorderDash(double[] borderDashArray, double borderDashOffset)
		{
			_borderDashOffset = (float)borderDashOffset;

			if (borderDashArray != null && borderDashArray.Length > 0)
			{
				nfloat[] dashArray;
				double[] array;

				if (borderDashArray.Length % 2 == 0)
				{
					array = new double[borderDashArray.Length];
					dashArray = new nfloat[borderDashArray.Length];
					borderDashArray.CopyTo(array, 0);
				}
				else
				{
					array = new double[2 * borderDashArray.Length];
					dashArray = new nfloat[2 * borderDashArray.Length];
					borderDashArray.CopyTo(array, 0);
					borderDashArray.CopyTo(array, borderDashArray.Length);
				}

				double thickness = _borderWidth;

				for (int i = 0; i < array.Length; i++)
					dashArray[i] = new nfloat(thickness * array[i]);

				_borderDash = dashArray;
			}

			SetNeedsDisplay();
		}

		CGPath? GetClipPath()
		{
			if (_shape != null)
			{
				var bounds = Bounds.ToRectangle();
				var path = _shape.PathForBounds(bounds);
				return path?.AsCGPath();
			}

			return null;
		}

		void SetDefaultBackgroundColor()
		{
			_backgroundColor = UIColor.Clear;
		}

		void DrawBackground(CGContext ctx)
		{
			if (_background != null)
				DrawGradientPaint(ctx, _background);
			else if (_backgroundColor != null)
			{
				ctx.SetFillColor(_backgroundColor.CGColor);
				var clipPath = GetClipPath();

				if (clipPath != null)
					ctx.AddPath(clipPath);

				ctx.DrawPath(CGPathDrawingMode.Fill);
			}
		}

		void DrawBorder(CGContext ctx)
		{
			if (_borderWidth == 0)
				return;

			if (IsBorderDashed())
				ctx.SetLineDash(_borderDashOffset * _borderWidth, _borderDash);

			ctx.SetLineWidth(_borderWidth);
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
						Graphics.Color color = gradientPaint.GradientStops[index].Color;
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

		bool IsBorderDashed()
		{
			return _borderDash != null && _borderDashOffset > 0;
		}
	}
}