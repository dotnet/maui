﻿using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Util;
using AndroidX.Core.Content;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using AColor = Android.Graphics.Color;
using AContext = Android.Content.Context;
using APaint = Android.Graphics.Paint;
using ARect = Android.Graphics.Rect;
using GPaint = Microsoft.Maui.Graphics.Paint;

namespace Microsoft.Maui.Platform
{
	public class BorderDrawable : PaintDrawable
	{
		readonly AContext? _context;
		readonly float _density;

		bool _invalidatePath;

		bool _disposed;

		ARect? _bounds;
		int _width;
		int _height;

		Path? _clipPath;
		APaint? _borderPaint;

		GPaint? _background;
		AColor? _backgroundColor;

		GPaint? _stroke;
		AColor? _borderColor;

		float _strokeThickness;

		CornerRadius _cornerRadius;

		public BorderDrawable(AContext? context)
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

		public void SetCornerRadius(CornerRadius cornerRadius)
		{
			if (_cornerRadius == cornerRadius)
				return;

			_invalidatePath = true;

			_cornerRadius = cornerRadius;

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

		protected override void OnBoundsChange(ARect bounds)
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

			if (Paint != null)
				SetBackground(Paint);

			if (HasBorder())
			{
				if (_borderPaint != null)
				{
					_borderPaint.StrokeWidth = _strokeThickness;

					if (_borderColor != null)
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
						_borderPaint.Color = _borderColor.Value;
#pragma warning restore CA1416
					else
					{
						if (_stroke != null)
							SetPaint(_borderPaint, _stroke);
					}
				}
			}

			if (_invalidatePath)
			{
				_invalidatePath = false;

				// TODO: Should this do the same thing as MauiDrawable.Android.cs? I suspect it should.
				var path = GetPath(_width, _height, _cornerRadius, _strokeThickness);
				var clipPath = path?.AsAndroidPath();

				if (clipPath == null)
					return;

				if (_clipPath != null)
				{
					_clipPath.Reset();
					_clipPath.Set(clipPath);
				}
			}

			if (canvas == null)
				return;

			var saveCount = canvas.SaveLayer(0, 0, _width, _height, null);

			if (_clipPath != null && Paint != null)
				canvas.DrawPath(_clipPath, Paint);

			if (HasBorder())
			{
				if (_clipPath != null && _borderPaint != null)
					canvas.DrawPath(_clipPath, _borderPaint);
			}

			canvas.RestoreToCount(saveCount);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_clipPath?.Dispose();
				_clipPath = null;
			}

			DisposeBorder(disposing);

			base.Dispose(disposing);
		}

		protected virtual void DisposeBorder(bool disposing)
		{
			if (disposing)
			{
				_borderPaint?.Dispose();
				_borderPaint = null;
			}
		}

		bool HasBorder()
		{
			InitializeBorderIfNeeded();

			return _stroke != null || _borderColor != null;
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
					var type = resource?.ToLowerInvariant();

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
				if (_backgroundColor != null)
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-android/issues/6962
					platformPaint.Color = _backgroundColor.Value;
#pragma warning restore CA1416
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

		static GradientData GetGradientPaintData(GradientPaint gradientPaint)
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

		PathF GetPath(float width, float height, CornerRadius cornerRadius, float strokeThickness)
		{
			var path = new PathF();

			float x = (float)strokeThickness / 2;
			float y = (float)strokeThickness / 2;

			float w = (float)(width - strokeThickness);
			float h = (float)(height - strokeThickness);

			float topLeftCornerRadius = _context.ToPixels(cornerRadius.TopLeft);
			float topRightCornerRadius = _context.ToPixels(cornerRadius.TopRight);
			float bottomLeftCornerRadius = _context.ToPixels(cornerRadius.BottomLeft);
			float bottomRightCornerRadius = _context.ToPixels(cornerRadius.BottomRight);

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}
	}
}