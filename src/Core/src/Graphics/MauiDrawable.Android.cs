using System;
using System.Linq;
using Android.Graphics;
using static Android.Graphics.Paint;
using AColor = Android.Graphics.Color;
using AContext = Android.Content.Context;
using APaint = Android.Graphics.Paint;
using GPaint = Microsoft.Maui.Graphics.Paint;

namespace Microsoft.Maui.Graphics
{
	public class MauiDrawable : PlatformDrawable
	{
		static Join? JoinMiter;
		static Join? JoinBevel;
		static Join? JoinRound;

		static Cap? CapButt;
		static Cap? CapSquare;
		static Cap? CapRound;

		readonly AContext? _context;

		// Cache values on .NET side to avoid unnecessary calls to Java
		GPaint? _background;
		GPaint? _stroke;
		IShape? _shape;
		double _strokeWidth;
		float _strokeMiterLimit;
		Join? _strokeLineJoin;
		Cap? _strokeLineCap;
		float[]? _strokeDashArray;
		float _strokeThickness;
		double _strokeDashOffset;

		public MauiDrawable(AContext? context) : base(context)
		{
			_context = context;
			
			// Initialize cached values
			_strokeWidth = 0;
			_strokeMiterLimit = 0;
			_strokeLineJoin = null;
			_strokeLineCap = null;
		}

		[Obsolete("Use `SetBackground(Microsoft.Maui.Graphics.Paint paint)` instead.")]
		public void SetBackgroundColor(AColor? backgroundColor)
		{
			if (backgroundColor is null)
			{
				SetBackground((GPaint?)null);
			}
			else
			{
				var solidPaint = new SolidPaint(backgroundColor.Value.ToColor());
				SetBackground(solidPaint);
			}
		}

		public void SetBackground(GPaint? paint)
		{
			switch (paint)
			{
				case SolidPaint solidPaint:
					SetBackground(solidPaint);
					break;
				case LinearGradientPaint linearGradientPaint:
					SetBackground(linearGradientPaint);
					break;
				case RadialGradientPaint radialGradientPaint:
					SetBackground(radialGradientPaint);
					break;
				case ImagePaint imagePaint:
					SetBackground(imagePaint);
					break;
				case PatternPaint patternPaint:
					SetBackground(patternPaint);
					break;
				default:
					SetDefaultBackgroundColor();
					break;
			}
		}

		public void SetBackground(SolidPaint solidPaint)
		{
			if (_background == solidPaint)
			{
				return;
			}

			_background = solidPaint;
			
			if (solidPaint.Color is { } color)
			{
				SetBackgroundColor(PlatformPaintType.Solid, solidPaint.IsSolid(), color.ToPlatform());
			}
			else
			{
				SetDefaultBackgroundColor();
			}
		}

		public void SetBackground(LinearGradientPaint linearGradientPaint)
		{
			if (_background == linearGradientPaint)
			{
				return;
			}

			_background = linearGradientPaint;
			
			var isSolid = linearGradientPaint.IsSolid();
			var gradientData = linearGradientPaint.GetGradientData(1.0f);
			
			SetBackgroundStyle(PlatformPaintType.Linear, isSolid, gradientData.Colors, gradientData.Offsets,
				[gradientData.X1, gradientData.Y1, gradientData.X2, gradientData.Y2]);
		}

		public void SetBackground(RadialGradientPaint radialGradientPaint)
		{
			if (_background == radialGradientPaint)
			{
				return;
			}

			_background = radialGradientPaint;
			
			var isSolid = radialGradientPaint.IsSolid();
			var gradientData = radialGradientPaint.GetGradientData(1.0f);

			SetBackgroundStyle(PlatformPaintType.Radial, isSolid, gradientData.Colors, gradientData.Offsets,
				[gradientData.CenterX, gradientData.CenterY, gradientData.Radius]);
		}

		public void SetBackground(ImagePaint imagePaint)
		{
			throw new NotImplementedException();
		}

		public void SetBackground(PatternPaint patternPaint)
		{
			throw new NotImplementedException();
		}

		void SetDefaultBackgroundColor()
		{
			if (_background is null)
			{
				return;
			}

			_background = null;

			SetBackgroundColor(PlatformPaintType.None, false, 0);
		}

		public void SetBorderShape(IShape? shape)
		{
			if (_shape == shape)
			{
				return;
			}

			_shape = shape;

			ShapeChanged(shape != null);
		}

		[Obsolete("Use `SetBorderBrush(Microsoft.Maui.Graphics.Paint paint)` instead.")]
		public void SetBorderColor(AColor? borderColor)
		{
			if (borderColor is null)
			{
				SetEmptyBorderBrush();
			}
			else
			{
				var solidPaint = new SolidPaint(borderColor.Value.ToColor());
				SetBorderBrush(solidPaint);
			}
		}

		public void SetBorderBrush(GPaint? paint)
		{
			switch (paint)
			{
				case SolidPaint solidPaint:
					SetBorderBrush(solidPaint);
					break;
				case LinearGradientPaint linearGradientPaint:
					SetBorderBrush(linearGradientPaint);
					break;
				case RadialGradientPaint radialGradientPaint:
					SetBorderBrush(radialGradientPaint);
					break;
				case ImagePaint imagePaint:
					SetBorderBrush(imagePaint);
					break;
				case PatternPaint patternPaint:
					SetBorderBrush(patternPaint);
					break;
				default:
					SetEmptyBorderBrush();
					break;
			}
		}

		public void SetBorderBrush(SolidPaint solidPaint)
		{
			if (_stroke == solidPaint)
			{
				return;
			}
			
			_stroke = solidPaint;
			
			if (solidPaint.Color is { } color)
			{
				SetBorderColor(PlatformPaintType.Solid, solidPaint.IsSolid(), color.ToPlatform());
			}
			else
			{
				SetEmptyBorderBrush();
			}
		}

		public void SetBorderBrush(LinearGradientPaint linearGradientPaint)
		{
			if (_stroke == linearGradientPaint)
			{
				return;
			}
			
			_stroke = linearGradientPaint;
			
			var isSolid = linearGradientPaint.IsSolid();
			var gradientData = linearGradientPaint.GetGradientData(1.0f);
			
			SetBorderStyle(PlatformPaintType.Linear, isSolid, gradientData.Colors, gradientData.Offsets,
				[gradientData.X1, gradientData.Y1, gradientData.X2, gradientData.Y2]);
		}

		public void SetBorderBrush(RadialGradientPaint radialGradientPaint)
		{
			if (_stroke == radialGradientPaint)
			{
				return;
			}
			
			_stroke = radialGradientPaint;
			
			var isSolid = radialGradientPaint.IsSolid();
			var gradientData = radialGradientPaint.GetGradientData(1.0f);

			SetBorderStyle(PlatformPaintType.Radial, isSolid, gradientData.Colors, gradientData.Offsets,
				[gradientData.CenterX, gradientData.CenterY, gradientData.Radius]);
		}

		public void SetEmptyBorderBrush()
		{
			if (_stroke is null)
			{
				return;
			}
			
			_stroke = null;

			SetBorderColor(PlatformPaintType.None, false, 0);
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
			if (_strokeWidth == strokeWidth)
			{
				return;
			}

			_strokeWidth = strokeWidth;

			float density = _context.GetDisplayDensity();
			var strokeThickness = (float)(strokeWidth * density);
			_strokeThickness = strokeThickness;
			StrokeThickness = strokeThickness;
		}

		public void SetBorderDash(float[]? strokeDashArray, double strokeDashOffset)
		{
			if (strokeDashArray == _strokeDashArray && _strokeDashOffset == strokeDashOffset)
			{
				return;
			}
			
			_strokeDashArray = strokeDashArray;
			_strokeDashOffset = strokeDashOffset;
			
			PathEffect? pathEffect = null;
			if (strokeDashArray is not null && strokeDashArray.Length > 0)
			{
				float[] strokeDash = new float[strokeDashArray.Length];
				float strokeThickness = _strokeThickness;

				for (int i = 0; i < strokeDashArray.Length; i++)
				{
					strokeDash[i] = strokeDashArray[i] * strokeThickness;
				}

				if (strokeDash.Length > 1)
				{
					pathEffect = new DashPathEffect(strokeDash, (float)strokeDashOffset * strokeThickness);
				}
			}

			BorderPathEffect = pathEffect;
		}

		public void SetBorderMiterLimit(float strokeMiterLimit)
		{
			if (_strokeMiterLimit == strokeMiterLimit)
			{
				return;
			}

			_strokeMiterLimit = strokeMiterLimit;

			StrokeMiterLimit = strokeMiterLimit;
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
			{
				return;
			}

			_strokeLineJoin = aLineJoin;

			StrokeLineJoin = aLineJoin;
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
			{
				return;
			}

			_strokeLineCap = aLineCap;

			StrokeLineCap = aLineCap;
		}

		public void InvalidateBorderBounds()
		{
			InvalidateSelf();
		}

		internal override void UpdateClipPath(int width, int height)
		{
			if (_shape is null)
			{
				return;
			}

			float density = _context.GetDisplayDensity();
			float strokeWidth = (float)(_strokeThickness / density);
			float fw = width / density;
			float w = fw - strokeWidth;
			float fh = height / density;
			float h = fh - strokeWidth;
			float x = strokeWidth / 2;
			float y = strokeWidth / 2;

			var bounds = new Rect(x, y, w, h);
			var clipPath = _shape.ToPlatform(bounds, strokeWidth, density);

			ClipPath = clipPath;

			var fullClipPath = _shape.ToPlatform(new Rect(0, 0, fw, fh), 0, density);
			FullClipPath = fullClipPath;
		}

		[Obsolete("This was part of internal logic and was public by mistake, it will be removed in the next major version.")]
		public void SetRadialGradientPaint(APaint platformPaint, RadialGradientPaint radialGradientPaint)
		{
			// No-op - functionality moved to Java
		}

		[Obsolete("This was part of internal logic and was public by mistake, it will be removed in the next major version.")]
		protected virtual void DisposeBorder(bool disposing)
		{
			// No-op - functionality moved to Java
		}
	}
}