using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using AColor = Android.Graphics.Color;
using ARect = Android.Graphics.Rect;
using APaint = Android.Graphics.Paint;
using GPaint = Microsoft.Maui.Graphics.Paint;

namespace Microsoft.Maui.Graphics
{
	public class MauiDrawable : PaintDrawable
	{
		readonly double _density;

		bool _invalidatePath;

		bool _disposed;

		int _width;
		int _height;

		Path? _clipPath;
		Path? _maskPath;
		APaint? _maskPaint;
		APaint? _borderPaint;

		CornerRadius _cornerRadius;

		GPaint? _background;
		AColor? _backgroundColor;

		GPaint? _border;
		AColor? _borderColor;

		float _borderWidth;

		public MauiDrawable(Context? context)
		{
			Shape = new RectShape();

			_invalidatePath = true;

			_clipPath = new Path();

			_density = context?.Resources?.DisplayMetrics?.Density ?? 1.0f;
		}

		public void SetBackgroundColor(AColor? backgroundColor)
		{
			_backgroundColor = backgroundColor;

			InvalidateSelf();
		}

		public void SetBackground(GPaint? paint)
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
			_invalidatePath = true;
			_backgroundColor = null;

			var color = solidPaint.Color == null
				? (AColor?)null
				: solidPaint.Color.ToNative();

			_background = null;
			SetBackgroundColor(color);
		}

		public void SetBackground(LinearGradientPaint linearGradientPaint)
		{
			_invalidatePath = true;

			_backgroundColor = null;
			_background = linearGradientPaint;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBackground(RadialGradientPaint radialGradientPaint)
		{
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

		public void SetCornerRadius(CornerRadius cornerRadius)
		{
			_invalidatePath = true;

			_cornerRadius = cornerRadius;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBorderColor(AColor? borderColor)
		{
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
				: solidPaint.Color.ToNative();

			_border = null;
			SetBorderColor(borderColor);
		}

		public void SetBorderBrush(LinearGradientPaint linearGradientPaint)
		{
			_invalidatePath = true;

			_borderColor = null;
			_border = linearGradientPaint;

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		public void SetBorderBrush(RadialGradientPaint radialGradientPaint)
		{
			_invalidatePath = true;

			_borderColor = null;
			_border = radialGradientPaint;

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
			_invalidatePath = true;

			_borderWidth = (float)(strokeWidth * _density);

			InitializeBorderIfNeeded();
			InvalidateSelf();
		}

		protected override void OnBoundsChange(ARect? bounds)
		{
			if (bounds != null)
			{
				var width = bounds.Width();
				var height = bounds.Height();

				if (_width == width && _height == height)
					return;

				_invalidatePath = true;

				_width = width;
				_height = height;
			}

			base.OnBoundsChange(bounds);
		}

		protected override void OnDraw(Shape? shape, Canvas? canvas, APaint? paint)
		{
			if (_disposed)
				return;

			if (Paint != null)
			{
				if (_backgroundColor != null)
					Paint.Color = _backgroundColor.Value;
				else
				{
					if (_background != null)
						SetPaint(Paint, _background);
				}
			}

			if (_borderPaint != null && HasBorder())
			{
				_borderPaint.StrokeWidth = _borderWidth;

				if (_borderColor != null)
					_borderPaint.Color = _borderColor.Value;
				else
				{
					if (_border != null)
						SetPaint(_borderPaint, _border);
				}
			}
			
			if (_invalidatePath)
			{
				_invalidatePath = false;

				var clipPath = GetRoundCornersPath(_width, _height, _borderWidth, _cornerRadius);

				if (clipPath == null)
					return;

				if (_clipPath != null)
				{
					_clipPath.Reset();
					_clipPath.Set(clipPath);

					if (_maskPath != null && HasBorder())
					{
						_maskPath.Reset();
						_maskPath.AddRect(0, 0, _width, _height, Path.Direction.Cw!);
						_maskPath.InvokeOp(_clipPath, Path.Op.Difference!);
					}
				}
			}

			if (canvas == null)
				return;

			if (HasBorder())
			{
				var saveCount = canvas.SaveLayer(0, 0, _width, _height, null);

				if (_clipPath != null && Paint != null)
					canvas.DrawPath(_clipPath, Paint);

				if (_clipPath != null && _borderPaint != null)
					canvas.DrawPath(_clipPath, _borderPaint);

				if (_maskPath != null && _maskPaint != null)
					canvas.DrawPath(_maskPath, _maskPaint);

				canvas.RestoreToCount(saveCount);
			}
			else
			{
				if (_clipPath != null && Paint != null)
					canvas.DrawPath(_clipPath, Paint);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
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
				if (_maskPath != null)
				{
					_maskPath.Dispose();
					_maskPath = null;
				}

				if (_maskPaint != null)
				{
					_maskPaint.SetXfermode(null);
					_maskPaint.Dispose();
					_maskPaint = null;
				}

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

			return _borderWidth > 0;
		}

		void InitializeBorderIfNeeded()
		{
			if (_borderWidth == 0)
			{
				DisposeBorder(true);
				return;
			}

			if (_maskPath == null)
				_maskPath = new Path();

			if (_maskPaint == null)
			{
				_maskPaint = new APaint(PaintFlags.AntiAlias);
				_maskPaint.SetStyle(APaint.Style.FillAndStroke);

				PorterDuffXfermode porterDuffClearMode = new PorterDuffXfermode(PorterDuff.Mode.Clear);
				_maskPaint.SetXfermode(porterDuffClearMode);
			}

			if (_borderPaint == null)
			{
				_borderPaint = new APaint(PaintFlags.AntiAlias);
				_borderPaint.SetStyle(APaint.Style.Stroke);
			}
		}

		Path GetRoundCornersPath(int width, int height, float borderWidth, CornerRadius cornerRadius)
		{
			var path = new Path();

			var cornerRadii = new[]
			{
				ToPixels(cornerRadius.TopLeft, _density),
				ToPixels(cornerRadius.TopLeft, _density),

				ToPixels(cornerRadius.TopRight, _density),
				ToPixels(cornerRadius.TopRight, _density),

				ToPixels(cornerRadius.BottomRight, _density),
				ToPixels(cornerRadius.BottomRight, _density),

				ToPixels(cornerRadius.BottomLeft, _density),
				ToPixels(cornerRadius.BottomLeft, _density)
			};

			var xPlatBorderWidth = (float)(borderWidth / _density / 2);

			path.AddRoundRect(xPlatBorderWidth, xPlatBorderWidth, width - xPlatBorderWidth, height - xPlatBorderWidth, cornerRadii, Path.Direction.Cw!);

			return path;
		}

		float ToPixels(double units, double density) =>
			(float)(units * density);

		void SetPaint(APaint nativePaint, GPaint paint)
		{
			if (paint is LinearGradientPaint linearGradientPaint)
				SetLinearGradientPaint(nativePaint, linearGradientPaint);

			if (paint is RadialGradientPaint radialGradientPaint)
				SetRadialGradientPaint(nativePaint, radialGradientPaint);
		}

		void SetLinearGradientPaint(APaint nativePaint, LinearGradientPaint linearGradientPaint)
		{
			var p1 = linearGradientPaint.StartPoint;
			var x1 = (float)p1.X;
			var y1 = (float)p1.Y;

			var p2 = linearGradientPaint.EndPoint;
			var x2 = (float)p2.X;
			var y2 = (float)p2.Y;

			var gradientData = GetGradientPaintData(linearGradientPaint);
			var linearGradientData = new LinearGradientData(gradientData.Colors, gradientData.Offsets, x1, y1, x2, y2);

			if (_width == 0 && _height == 0)
				return;

			if (linearGradientData.Colors == null || linearGradientData.Colors.Length < 2)
				return;

			var linearGradientShader = new LinearGradient(
				_width * linearGradientData.X1,
				_height * linearGradientData.Y1,
				_width * linearGradientData.X2,
				_height * linearGradientData.Y2,
				linearGradientData.Colors,
				linearGradientData.Offsets,
				Shader.TileMode.Clamp!);

			nativePaint.SetShader(linearGradientShader);
		}

		public void SetRadialGradientPaint(APaint nativePaint, RadialGradientPaint radialGradientPaint)
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

			nativePaint.SetShader(radialGradient);
		}

		GradientData GetGradientPaintData(GradientPaint gradientPaint)
		{
			var orderStops = gradientPaint.GradientStops;

			var data = new GradientData(orderStops.Length);

			int count = 0;
			foreach (var orderStop in orderStops)
			{
				data.Colors[count] = orderStop.Color.ToNative().ToArgb();
				data.Offsets[count] = orderStop.Offset;
				count++;
			}

			return data;
		}
	}
}