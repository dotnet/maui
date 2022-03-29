using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Util;
using AndroidX.Core.Content;
using Microsoft.Maui.Graphics.Platform;
using static Android.Graphics.Paint;
using AColor = Android.Graphics.Color;
using AContext = Android.Content.Context;
using APaint = Android.Graphics.Paint;
using ARect = Android.Graphics.Rect;
using GPaint = Microsoft.Maui.Graphics.Paint;

namespace Microsoft.Maui.Graphics
{
	public class MauiDrawable : PaintDrawable
	{
		readonly AContext? _context;
		readonly double _density;

		bool _invalidatePath;

		bool _disposed;

		ARect? _bounds;
		int _width;
		int _height;

		Path? _clipPath;
		APaint? _borderPaint;

		IShape? _shape;

		GPaint? _background;
		AColor? _backgroundColor;

		GPaint? _stroke;
		AColor? _borderColor;
		PathEffect? _borderPathEffect;

		Join? _strokeLineJoin;
		Cap? _strokeLineCap;

		float _strokeThickness;
		float _strokeMiterLimit;

		public MauiDrawable(AContext? context)
		{
			Shape = new RectShape();

			_invalidatePath = true;

			_clipPath = new Path();

			_context = context;
			_density = context?.Resources?.DisplayMetrics?.Density ?? 1.0f;
		}

		public void SetBackgroundColor(AColor? backgroundColor)
		{
			if (_backgroundColor == backgroundColor)
				return;

			_backgroundColor = backgroundColor;

			InvalidateSelf();
		}

		public void SetBackground(GPaint? paint)
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
			_invalidatePath = true;
			_backgroundColor = null;
			_background = null;

			if (solidPaint.Color == null)
				SetDefaultBackgroundColor();
			else
			{
				var backgroundColor = solidPaint.Color.ToPlatform();
				SetBackgroundColor(backgroundColor);
			}
		}

		public void SetBackground(LinearGradientPaint linearGradientPaint)
		{
			if (_background == linearGradientPaint)
				return;

			_invalidatePath = true;

			_backgroundColor = null;
			_background = linearGradientPaint;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBackground(RadialGradientPaint radialGradientPaint)
		{
			if (_background == radialGradientPaint)
				return;

			_invalidatePath = true;

			_backgroundColor = null;
			_background = radialGradientPaint;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBackground(ImagePaint imagePaint)
		{
			throw new NotImplementedException();
		}

		public void SetBackground(PatternPaint patternPaint)
		{
			throw new NotImplementedException();
		}

		public void SetBorderShape(IShape? shape)
		{
			if (_shape == shape)
				return;

			_invalidatePath = true;

			_shape = shape;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBorderColor(AColor? borderColor)
		{
			if (_borderColor == borderColor)
				return;

			_borderColor = borderColor;

			InvalidateSelf();
		}

		public void SetBorderBrush(GPaint? paint)
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
			_invalidatePath = true;
			_borderColor = null;

			var borderColor = solidPaint.Color == null
				? (AColor?)null
				: solidPaint.Color.ToPlatform();

			_stroke = null;
			SetBorderColor(borderColor);
		}

		public void SetBorderBrush(LinearGradientPaint linearGradientPaint)
		{
			if (_stroke == linearGradientPaint)
				return;

			_invalidatePath = true;

			_borderColor = null;
			_stroke = linearGradientPaint;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBorderBrush(RadialGradientPaint radialGradientPaint)
		{
			if (_stroke == radialGradientPaint)
				return;

			_invalidatePath = true;

			_borderColor = null;
			_stroke = radialGradientPaint;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBorderBrush(ImagePaint imagePaint)
		{
			throw new NotImplementedException();
		}

		public void SetBorderBrush(PatternPaint patternPaint)
		{
			throw new NotImplementedException();
		}

		public void SetBorderWidth(double strokeWidth)
		{
			float strokeThickness = (float)(strokeWidth * _density);

			if (_strokeThickness == strokeThickness)
				return;

			_invalidatePath = true;

			_strokeThickness = strokeThickness;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBorderDash(float[]? strokeDashArray, double strokeDashOffset)
		{
			if (strokeDashArray == null || strokeDashArray.Length == 0 || strokeDashOffset <= 0)
				_borderPathEffect = null;
			else
			{
				float[] strokeDash = new float[strokeDashArray.Length];

				for (int i = 0; i < strokeDashArray.Length; i++)
					strokeDash[i] = strokeDashArray[i] * _strokeThickness;

				if (strokeDash.Length > 1)
					_borderPathEffect = new DashPathEffect(strokeDash, (float)strokeDashOffset * _strokeThickness);
			}

			InvalidateSelf();
		}

		public void SetBorderMiterLimit(float strokeMiterLimit)
		{
			if (_strokeMiterLimit == strokeMiterLimit)
				return;

			_strokeMiterLimit = strokeMiterLimit;

			InvalidateSelf();
		}

		public void SetBorderLineJoin(LineJoin lineJoin)
		{
			Join? aLineJoin = Join.Miter;

			switch (lineJoin)
			{
				case LineJoin.Miter:
					aLineJoin = Join.Miter;
					break;
				case LineJoin.Bevel:
					aLineJoin = Join.Bevel;
					break;
				case LineJoin.Round:
					aLineJoin = Join.Round;
					break;
			}

			if (_strokeLineJoin == aLineJoin)
				return;

			_strokeLineJoin = aLineJoin;

			InvalidateSelf();
		}

		public void SetBorderLineCap(LineCap lineCap)
		{
			Cap? aLineCap = Cap.Butt;

			switch (lineCap)
			{
				case LineCap.Butt:
					aLineCap = Cap.Butt;
					break;
				case LineCap.Square:
					aLineCap = Cap.Square;
					break;
				case LineCap.Round:
					aLineCap = Cap.Round;
					break;
			}

			if (_strokeLineCap == aLineCap)
				return;

			_strokeLineCap = aLineCap;

			InvalidateSelf();
		}

		protected override void OnBoundsChange(ARect? bounds)
		{
			if (_bounds != bounds)
			{
				_bounds = bounds;

				if (_bounds != null)
				{
					var width = _bounds.Width();
					var height = _bounds.Height();

					if (_width == width && _height == height)
						return;

					_invalidatePath = true;

					_width = width;
					_height = height;
				}
			}

			base.OnBoundsChange(bounds);
		}

		protected override void OnDraw(Shape? shape, Canvas? canvas, APaint? paint)
		{
			if (_disposed)
				return;

			if (HasBorder())
			{
				if (Paint != null)
					SetBackground(Paint);

				if (_borderPaint != null)
				{
					_borderPaint.StrokeWidth = _strokeThickness;
					_borderPaint.StrokeJoin = _strokeLineJoin;
					_borderPaint.StrokeCap = _strokeLineCap;
					_borderPaint.StrokeMiter = _strokeMiterLimit * 2;

					if (_borderPathEffect != null)
						_borderPaint.SetPathEffect(_borderPathEffect);

					if (_borderColor != null && OperatingSystem.IsAndroidVersionAtLeast(29))
						_borderPaint.Color = _borderColor.Value;
					else
					{
						if (_stroke != null)
							SetPaint(_borderPaint, _stroke);
					}
				}

				if (_invalidatePath)
				{
					_invalidatePath = false;

					if (_shape != null)
					{
						var bounds = new Graphics.Rect(0, 0, _width, _height);
						var path = _shape.PathForBounds(bounds);
						var clipPath = path?.AsAndroidPath();

						if (clipPath == null)
							return;

						if (_clipPath != null)
						{
							_clipPath.Reset();
							_clipPath.Set(clipPath);
						}
					}
				}

				if (canvas == null)
					return;

				var saveCount = canvas.SaveLayer(0, 0, _width, _height, null);

				if (_clipPath != null && Paint != null)
					canvas.DrawPath(_clipPath, Paint);

				if (_clipPath != null && _borderPaint != null)
					canvas.DrawPath(_clipPath, _borderPaint);

				canvas.RestoreToCount(saveCount);
			}
			else
			{
				if (paint != null)
					SetBackground(paint);

				base.OnDraw(shape, canvas, paint);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_borderPathEffect != null)
				{
					_borderPathEffect.Dispose();
					_borderPathEffect = null;
				}

				if (_clipPath != null)
				{
					_clipPath.Dispose();
					_clipPath = null;
				}
			}

			DisposeBorder(disposing);

			base.Dispose(disposing);
		}

		protected virtual void DisposeBorder(bool disposing)
		{
			if (disposing)
			{
				if (_borderPaint != null)
				{
					_borderPaint.Dispose();
					_borderPaint = null;
				}
			}
		}

		bool HasBorder()
		{
			InitializeBorderIfNeeded();

			return _shape != null && (_stroke != null || _borderColor != null);
		}

		void InitializeBorderIfNeeded()
		{
			if (_strokeThickness == 0)
			{
				DisposeBorder(true);
				return;
			}

			if (_borderPaint == null)
			{
				_borderPaint = new APaint(PaintFlags.AntiAlias);
				_borderPaint.SetStyle(APaint.Style.Stroke);
			}
		}

		void SetDefaultBackgroundColor()
		{
			using (var background = new TypedValue())
			{
				if (_context == null || _context.Theme == null || _context.Resources == null)
					return;

				if (_context.Theme.ResolveAttribute(global::Android.Resource.Attribute.WindowBackground, background, true))
				{
					var resource = _context.Resources.GetResourceTypeName(background.ResourceId);
					var type = resource?.ToLower();

					if (type == "color")
					{
						var color = new AColor(ContextCompat.GetColor(_context, background.ResourceId));
						_backgroundColor = color;
					}
				}
			}
		}

		void SetBackground(APaint platformPaint)
		{
			if (platformPaint != null)
			{
				if (_backgroundColor != null && OperatingSystem.IsAndroidVersionAtLeast(29))
					platformPaint.Color = _backgroundColor.Value;
				else
				{
					if (_background != null)
						SetPaint(platformPaint, _background);
				}
			}
		}

		void SetPaint(APaint platformPaint, GPaint paint)
		{
			if (paint is LinearGradientPaint linearGradientPaint)
				SetLinearGradientPaint(platformPaint, linearGradientPaint);

			if (paint is RadialGradientPaint radialGradientPaint)
				SetRadialGradientPaint(platformPaint, radialGradientPaint);
		}

		void SetLinearGradientPaint(APaint platformPaint, LinearGradientPaint linearGradientPaint)
		{
			var p1 = linearGradientPaint.StartPoint;
			var x1 = (float)p1.X;
			var y1 = (float)p1.Y;
			var p2 = linearGradientPaint.EndPoint;
			var x2 = (float)p2.X;
			var y2 = (float)p2.Y;

			var data = GetGradientPaintData(linearGradientPaint);
			var shader = new LinearGradientData(data.Colors, data.Offsets, x1, y1, x2, y2);
			if (_width == 0 && _height == 0)
				return;

			if (shader.Colors == null || shader.Colors.Length < 2)
				return;

			var linearGradientShader = new LinearGradient(
				_width * shader.X1,
				_height * shader.Y1,
				_width * shader.X2,
				_height * shader.Y2,
				shader.Colors,
				shader.Offsets,
				Shader.TileMode.Clamp!);

			platformPaint.SetShader(linearGradientShader);
		}

		public void SetRadialGradientPaint(APaint platformPaint, RadialGradientPaint radialGradientPaint)
		{
			var center = radialGradientPaint.Center;
			float centerX = (float)center.X;
			float centerY = (float)center.Y;
			float radius = (float)radialGradientPaint.Radius;

			var gradientData = GetGradientPaintData(radialGradientPaint);
			var radialGradientData = new RadialGradientData(gradientData.Colors, gradientData.Offsets, centerX, centerY, radius);

			if (_width == 0 && _height == 0)
				return;

			if (radialGradientData.Colors == null || radialGradientData.Colors.Length < 2)
				return;

			var radialGradient = new RadialGradient(
				_width * radialGradientData.CenterX,
				_height * radialGradientData.CenterY,
				Math.Max(_height, _width) * radialGradientData.Radius,
				radialGradientData.Colors,
				radialGradientData.Offsets,
				Shader.TileMode.Clamp!);

			platformPaint.SetShader(radialGradient);
		}

		GradientData GetGradientPaintData(GradientPaint gradientPaint)
		{
			var orderStops = gradientPaint.GradientStops;

			var data = new GradientData(orderStops.Length);

			int count = 0;
			foreach (var orderStop in orderStops)
			{
				data.Colors[count] = orderStop.Color.ToPlatform().ToArgb();
				data.Offsets[count] = orderStop.Offset;
				count++;
			}

			return data;
		}
	}
}