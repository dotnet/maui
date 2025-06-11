#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiCALayer : CALayer, IAutoSizableCALayer
	{
		CGRect _bounds;
		WeakReference<IShape?> _shape;

		UIColor? _backgroundColor;
		Paint? _background;

		float _strokeThickness;
		UIColor? _strokeColor;
		Paint? _stroke;

		CGLineCap _strokeLineCap;
		CGLineJoin _strokeLineJoin;

		nfloat[]? _strokeDash;
		nfloat _strokeDashOffset;

		nfloat _strokeMiterLimit;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in CALayerAutosizeObserver_DoesNotLeak test.")]
		CALayerAutosizeObserver? _boundsObserver;

		public MauiCALayer()
		{
			_bounds = new CGRect();
			_shape = new WeakReference<IShape?>(null);
			ContentsScale = UIScreen.MainScreen.Scale;
		}

		protected override void Dispose(bool disposing)
		{
			_boundsObserver?.Dispose();
			_boundsObserver = null;
			base.Dispose(disposing);
		}

		public override void RemoveFromSuperLayer()
		{
			_boundsObserver?.Dispose();
			_boundsObserver = null;
			base.RemoveFromSuperLayer();
		}

		void IAutoSizableCALayer.AutoSizeToSuperLayer()
		{
			_boundsObserver?.Dispose();
			_boundsObserver = CALayerAutosizeObserver.Attach(this);
		}

		public override void AddAnimation(CAAnimation animation, string? key)
		{
			// Do nothing, we don't want animations here
		}

		public override void LayoutSublayers()
		{
			base.LayoutSublayers();

			// If the super layer's frame is zero, indicating an off-screen rendering scenario, 
			// the bounds are intentionally kept at zero to avoid incorrect initial drawing 
			// caused by bounds matching the screen size.
			var bounds = SuperLayer?.Frame == CGRect.Empty ? CGRect.Empty : Bounds;

			if (bounds.Equals(_bounds))
			{
				return;
			}

			_bounds = new CGRect(bounds.Location, bounds.Size);
		}

		public override void DrawInContext(CGContext ctx)
		{
			base.DrawInContext(ctx);

			var clipPath = GetClipPath();

			if (clipPath! != null!)
				ctx.AddPath(clipPath);

			ctx.Clip();

			DrawBackground(ctx);
			DrawBorder(ctx);
		}

		public void SetBorderShape(IShape? shape)
		{
			_shape = new WeakReference<IShape?>(shape);

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
				_backgroundColor = solidPaint.Color.ToPlatform();

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
			_strokeColor = solidPaint.Color == null
				? UIColor.Clear
				: solidPaint.Color.ToPlatform();

			_stroke = null;

			SetNeedsDisplay();
		}

		public void SetBorderBrush(LinearGradientPaint linearGradientPaint)
		{
			_strokeColor = null;
			_stroke = linearGradientPaint;

			SetNeedsDisplay();
		}

		public void SetBorderBrush(RadialGradientPaint radialGradientPaint)
		{
			_strokeColor = null;
			_stroke = radialGradientPaint;

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
			_strokeThickness = (float)borderWidth;

			SetNeedsDisplay();
		}

		public void SetBorderDash(float[]? borderDashArray, double borderDashOffset)
		{
			_strokeDashOffset = (float)borderDashOffset;

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

				double thickness = _strokeThickness;

				for (int i = 0; i < array.Length; i++)
					dashArray[i] = new nfloat(thickness * array[i]);

				_strokeDash = dashArray;
			}
			else if (borderDashArray is null)
			{
				_strokeDash = null;
			}

			SetNeedsDisplay();
		}

		public void SetBorderMiterLimit(float strokeMiterLimit)
		{
			_strokeMiterLimit = strokeMiterLimit;

			SetNeedsDisplay();
		}

		public void SetBorderLineJoin(LineJoin lineJoin)
		{
			CGLineJoin iLineJoin = CGLineJoin.Miter;

			switch (lineJoin)
			{
				case LineJoin.Miter:
					iLineJoin = CGLineJoin.Miter;
					break;
				case LineJoin.Bevel:
					iLineJoin = CGLineJoin.Bevel;
					break;
				case LineJoin.Round:
					iLineJoin = CGLineJoin.Round;
					break;
			}

			_strokeLineJoin = iLineJoin;

			SetNeedsDisplay();
		}

		public void SetBorderLineCap(LineCap lineCap)
		{
			CGLineCap iLineCap = CGLineCap.Butt;

			switch (lineCap)
			{
				case LineCap.Butt:
					iLineCap = CGLineCap.Butt;
					break;
				case LineCap.Square:
					iLineCap = CGLineCap.Square;
					break;
				case LineCap.Round:
					iLineCap = CGLineCap.Round;
					break;
			}

			_strokeLineCap = iLineCap;

			SetNeedsDisplay();
		}

		CGPath? GetClipPath()
		{
			if (_shape.TryGetTarget(out var shape))
			{
				var bounds = _bounds.ToRectangle();
				var path = shape.PathForBounds(bounds);
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

				if (clipPath! != null!)
					ctx.AddPath(clipPath);

				ctx.DrawPath(CGPathDrawingMode.Fill);
			}
		}

		void DrawBorder(CGContext ctx)
		{
			if (_strokeThickness <= 0)
				return;

			if (IsBorderDashed())
				ctx.SetLineDash(_strokeDashOffset * _strokeThickness, _strokeDash);

			// The Stroke is inner and we are clipping the outer, for that reason, we use the double to get the correct value.
			ctx.SetLineWidth(2 * _strokeThickness);

			ctx.SetLineCap(_strokeLineCap);
			ctx.SetLineJoin(_strokeLineJoin);
			ctx.SetMiterLimit(_strokeMiterLimit * _strokeThickness / 4);

			var clipPath = GetClipPath();

			if (clipPath! != null!)
				ctx.AddPath(clipPath);

			if (_stroke != null)
			{
				ctx.ReplacePathWithStrokedPath();
				ctx.Clip();

				DrawGradientPaint(ctx, _stroke);
			}
			else if (_strokeColor != null)
			{
				ctx.SetStrokeColor(_strokeColor.CGColor);
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
						var uiColor = new UIColor(new nfloat(color.Red), new nfloat(color.Green), new nfloat(color.Blue), new nfloat(color.Alpha));
						colors[index] = uiColor.CGColor;
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
			return _strokeDash != null;
		}
	}
}