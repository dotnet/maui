using System;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using static Android.Graphics.Paint;
using AColor = Android.Graphics.Color;
using AContext = Android.Content.Context;
using APaint = Android.Graphics.Paint;
using ARect = Android.Graphics.Rect;
using GPaint = Microsoft.Maui.Graphics.Paint;

namespace Microsoft.Maui.Graphics
{
	public class MauiDrawable : PaintDrawable, IPlatformShadowDrawable
	{
		static Join? JoinMiter;
		static Join? JoinBevel;
		static Join? JoinRound;

		static Cap? CapButt;
		static Cap? CapSquare;
		static Cap? CapRound;

		readonly AContext? _context;
		readonly float _density;

		bool _invalidatePath;
		bool _isBackgroundSolid;
		bool _isBorderSolid;

		bool _disposed;

		int _width;
		int _height;

		Path? _clipPath;
		Path? _fullClipPath;
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
			_fullClipPath = new Path();

			_context = context;
			_density = context.GetDisplayDensity();
		}

		public void SetBackgroundColor(AColor? backgroundColor)
		{
			if (_backgroundColor == backgroundColor)
				return;

			_backgroundColor = backgroundColor;
			_isBackgroundSolid = backgroundColor?.A is 255;

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

			var color = solidPaint.Color;
			if (color == null)
				SetDefaultBackgroundColor();
			else
			{
				var backgroundColor = color.ToPlatform();
				SetBackgroundColor(backgroundColor);
				_isBackgroundSolid = color.Alpha == 1;
			}
		}

		public void SetBackground(LinearGradientPaint linearGradientPaint)
		{
			if (_background == linearGradientPaint)
				return;

			_invalidatePath = true;

			_backgroundColor = null;
			_background = linearGradientPaint;
			_isBackgroundSolid = linearGradientPaint.GradientStops.All(s => s.Color.Alpha == 1);

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
			_isBackgroundSolid = radialGradientPaint.GradientStops.All(s => s.Color.Alpha == 1);

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
			_isBorderSolid = borderColor?.A is 255;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBorderBrush(GPaint? paint)
		{
			if (paint is SolidPaint solidPaint)
				SetBorderBrush(solidPaint);

			else if (paint is LinearGradientPaint linearGradientPaint)
				SetBorderBrush(linearGradientPaint);

			else if (paint is RadialGradientPaint radialGradientPaint)
				SetBorderBrush(radialGradientPaint);

			else if (paint is ImagePaint imagePaint)
				SetBorderBrush(imagePaint);

			else if (paint is PatternPaint patternPaint)
				SetBorderBrush(patternPaint);

			else
				SetEmptyBorderBrush();
		}

		public void SetBorderBrush(SolidPaint solidPaint)
		{
			_invalidatePath = true;
			_borderColor = null;
			_borderPaint = null;

			var color = solidPaint.Color;
			var borderColor = color?.ToPlatform();

			_stroke = null;
			SetBorderColor(borderColor);
			_isBorderSolid = solidPaint.IsSolid();
		}

		public void SetBorderBrush(LinearGradientPaint linearGradientPaint)
		{
			if (_stroke == linearGradientPaint)
				return;

			_invalidatePath = true;

			_borderColor = null;
			_stroke = linearGradientPaint;
			_isBorderSolid = linearGradientPaint.IsSolid();

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
			_isBorderSolid = radialGradientPaint.IsSolid();

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		// TODO: NET8 make public for net8.0
		internal void SetEmptyBorderBrush()
		{
			_invalidatePath = true;

			if (_backgroundColor != null)
			{
				_borderColor = _backgroundColor.Value;
				_stroke = null;
			}
			else
			{
				_borderColor = null;

				if (_background != null)
					SetBorderBrush(_background);
			}

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
			if (strokeDashArray is null || strokeDashArray.Length == 0)
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
			Join? aLineJoin = lineJoin switch
			{
				LineJoin.Miter => JoinMiter ??= Join.Miter,
				LineJoin.Bevel => JoinBevel ??= Join.Bevel,
				LineJoin.Round => JoinRound ??= Join.Round,
				_ => JoinMiter ??= Join.Miter,
			};

			if (_strokeLineJoin == aLineJoin)
				return;

			_strokeLineJoin = aLineJoin;

			InvalidateSelf();
		}

		public void SetBorderLineCap(LineCap lineCap)
		{
			Cap? aLineCap = lineCap switch
			{
				LineCap.Butt => CapButt ??= Cap.Butt,
				LineCap.Square => CapSquare ??= Cap.Square,
				LineCap.Round => CapRound ??= Cap.Round,
				_ => CapButt ??= Cap.Butt,
			};

			if (_strokeLineCap == aLineCap)
				return;

			_strokeLineCap = aLineCap;

			InvalidateSelf();
		}

		public void InvalidateBorderBounds()
		{
			InvalidateSelf();
		}

		protected override void OnBoundsChange(ARect bounds)
		{
			var width = bounds.Width();
			var height = bounds.Height();

			if (_width == width && _height == height)
				return;

			_invalidatePath = true;

			_width = width;
			_height = height;

			base.OnBoundsChange(bounds);
		}

		bool IPlatformShadowDrawable.CanDrawShadow()
		{
			return _isBackgroundSolid && (_strokeThickness == 0 || _isBorderSolid);
		}

		void IPlatformShadowDrawable.DrawShadow(Canvas? canvas, APaint? shadowPaint, Path? outerClipPath)
		{
			if (_disposed || canvas is null || shadowPaint is null)
				return;

			Path contentPath;

			if (HasBorder())
			{
				if (!TryUpdateClipPath() || _fullClipPath == null)
				{
					return;
				}

				contentPath = _fullClipPath;
			}
			else
			{
				contentPath = new Path();
				contentPath.AddRect(0, 0, _width, _height, Path.Direction.Cw!);
			}

			if (outerClipPath != null)
			{
				var clippedPath = new Path();
				clippedPath.InvokeOp(contentPath, outerClipPath, Path.Op.Intersect!);
				canvas.DrawPath(clippedPath, shadowPaint);
				clippedPath.Dispose();
			}
			else
			{
				canvas.DrawPath(contentPath, shadowPaint);
			}

			if (contentPath != _fullClipPath)
			{
				contentPath.Dispose();
			}
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
					PlatformInterop.SetPaintValues(_borderPaint, _strokeThickness, _strokeLineJoin, _strokeLineCap, _strokeMiterLimit * 2, _borderPathEffect);

					if (_borderColor != null)
					{
						_borderPaint.Color = _borderColor.Value;
					}
					else
					{
						if (_stroke != null)
							SetPaint(_borderPaint, _stroke);
					}
				}

				if (!TryUpdateClipPath())
				{
					return;
				}

				if (canvas == null || _clipPath == null)
					return;

				PlatformInterop.DrawMauiDrawablePath(this, canvas, _width, _height, _clipPath, _borderPaint);
			}
			else
			{
				if (paint != null)
					SetBackground(paint);

				base.OnDraw(shape, canvas, paint);
			}
		}

		bool TryUpdateClipPath()
		{
			if (_invalidatePath)
			{
				_invalidatePath = false;

				if (_shape != null)
				{
					float strokeThickness = _strokeThickness / _density;
					float fw = _width / _density;
					float w = fw - strokeThickness;
					float fh = _height / _density;
					float h = fh - strokeThickness;
					float x = strokeThickness / 2;
					float y = strokeThickness / 2;

					var bounds = new Rect(x, y, w, h);
					var clipPath = _shape?.ToPlatform(bounds, strokeThickness, _density);

					if (clipPath == null)
						return false;

					if (_clipPath != null)
					{
						_clipPath.Reset();
						_clipPath.Set(clipPath);
					}

					var fullClipPath = _shape!.ToPlatform(new Rect(0, 0, fw, fh), 0, _density);
					if (_fullClipPath != null)
					{
						_fullClipPath.Reset();
						_fullClipPath.Set(fullClipPath);
					}
				}
			}

			return true;
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

				if (_fullClipPath != null)
				{
					_fullClipPath.Dispose();
					_fullClipPath = null;
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

			return _shape != null;
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
			var color = PlatformInterop.GetWindowBackgroundColor(_context);
			if (color != -1)
			{
				var backgroundColor = new AColor(color);
				_backgroundColor = backgroundColor;
				_isBackgroundSolid = backgroundColor.IsSolid();
			}
		}

		void SetBackground(APaint platformPaint)
		{
			if (platformPaint != null)
			{
				if (_backgroundColor != null)
				{
					platformPaint.SetShader(null);
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
					platformPaint.Color = _backgroundColor.Value;
#pragma warning restore CA1416
				}
				else if (_background != null)
				{
					SetPaint(platformPaint, _background);
				}
				else
				{
					platformPaint.Color = AColor.Transparent;
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

		static GradientData GetGradientPaintData(GradientPaint gradientPaint)
		{
			var orderStops = gradientPaint.GradientStops;

			var data = new GradientData(orderStops.Length);

			int count = 0;
			foreach (var orderStop in orderStops.OrderBy(s => s.Offset))
			{
				data.Colors[count] = orderStop.Color.ToPlatform().ToArgb();
				data.Offsets[count] = orderStop.Offset;
				count++;
			}

			return data;
		}
	}
}