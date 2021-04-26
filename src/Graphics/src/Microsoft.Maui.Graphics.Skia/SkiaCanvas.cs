using System;
using Microsoft.Maui.Graphics.Text;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaCanvas : AbstractCanvas<SkiaCanvasState>, IBlurrableCanvas
	{
		private static SKPaint _defaultFillPaint;
		private static SKPaint _defaultFontPaint;
		private static SKPaint _defaultStrokePaint;

		private readonly SKMatrix _shaderMatrix = new SKMatrix();

		private SKCanvas _canvas;
		private float _displayScale = 1;
		private SKShader _shader;

		public SkiaCanvas() : base(CreateNewState, CreateStateCopy)
		{
		}

		public override float DisplayScale => _displayScale;

		public SKCanvas Canvas
		{
			get => _canvas;
			set
			{
				_canvas = null;
				ResetState();
				_canvas = value;
			}
		}

		public override bool Antialias
		{
			set => CurrentState.AntiAlias = value;
		}

		protected override float NativeStrokeSize
		{
			set => CurrentState.NativeStrokeSize = value;
		}

		public override float MiterLimit
		{
			set => CurrentState.MiterLimit = value;
		}

		public override float Alpha
		{
			set => CurrentState.Alpha = value;
		}

		public override LineCap StrokeLineCap
		{
			set => CurrentState.StrokeLineCap = value;
		}

		public override LineJoin StrokeLineJoin
		{
			set => CurrentState.StrokeLineJoin = value;
		}

		public override Color StrokeColor
		{
			set => CurrentState.StrokeColor = value ?? Colors.Black;
		}

		public override Color FontColor
		{
			set => CurrentState.FontColor = value ?? Colors.Black;
		}

		public override string FontName
		{
			set
			{
				if (value != null)
					CurrentState.FontName = value;
				else
					CurrentState.FontName = SkiaGraphicsService.Instance.SystemFontName;
			}
		}

		public override float FontSize
		{
			set => CurrentState.FontSize = value;
		}

		public override Color FillColor
		{
			set
			{
				if (_shader != null)
				{
					CurrentState.SetFillPaintShader(null);
					_shader.Dispose();
					_shader = null;
				}

				CurrentState.FillColor = value ?? Colors.White;
			}
		}

		public override BlendMode BlendMode
		{
			set
			{
				/* todo: implement this
				CGBlendMode blendMode = CGBlendMode.Normal;

				switch (value)
				{
					case BlendMode.Clear:
						blendMode = CGBlendMode.Clear;
						break;
					case BlendMode.Color:
						blendMode = CGBlendMode.Color;
						break;
					case BlendMode.ColorBurn:
						blendMode = CGBlendMode.ColorBurn;
						break;
					case BlendMode.ColorDodge:
						blendMode = CGBlendMode.ColorDodge;
						break;
					case BlendMode.Copy:
						blendMode = CGBlendMode.Copy;
						break;
					case BlendMode.Darken:
						blendMode = CGBlendMode.Darken;
						break;
					case BlendMode.DestinationAtop:
						blendMode = CGBlendMode.DestinationAtop;
						break;
					case BlendMode.DestinationIn:
						blendMode = CGBlendMode.DestinationIn;
						break;
					case BlendMode.DestinationOut:
						blendMode = CGBlendMode.DestinationOut;
						break;
					case BlendMode.DestinationOver:
						blendMode = CGBlendMode.DestinationOver;
						break;
					case BlendMode.Difference:
						blendMode = CGBlendMode.Difference;
						break;
					case BlendMode.Exclusion:
						blendMode = CGBlendMode.Exclusion;
						break;
					case BlendMode.HardLight:
						blendMode = CGBlendMode.HardLight;
						break;
					case BlendMode.Hue:
						blendMode = CGBlendMode.Hue;
						break;
					case BlendMode.Lighten:
						blendMode = CGBlendMode.Lighten;
						break;
					case BlendMode.Luminosity:
						blendMode = CGBlendMode.Luminosity;
						break;
					case BlendMode.Multiply:
						blendMode = CGBlendMode.Multiply;
						break;
					case BlendMode.Normal:
						blendMode = CGBlendMode.Normal;
						break;
					case BlendMode.Overlay:
						blendMode = CGBlendMode.Overlay;
						break;
					case BlendMode.PlusDarker:
						blendMode = CGBlendMode.PlusDarker;
						break;
					case BlendMode.PlusLighter:
						blendMode = CGBlendMode.PlusLighter;
						break;
					case BlendMode.Saturation:
						blendMode = CGBlendMode.Saturation;
						break;
					case BlendMode.Screen:
						blendMode = CGBlendMode.Screen;
						break;
					case BlendMode.SoftLight:
						blendMode = CGBlendMode.SoftLight;
						break;
					case BlendMode.SourceAtop:
						blendMode = CGBlendMode.SourceAtop;
						break;
					case BlendMode.SourceIn:
						blendMode = CGBlendMode.SourceIn;
						break;
					case BlendMode.SourceOut:
						blendMode = CGBlendMode.SourceOut;
						break;
					case BlendMode.XOR:
						blendMode = CGBlendMode.XOR;
						break;
				}

				canvas.SetBlendMode(blendMode);*/

				//CurrentState.FillPaint.SetXfermode(new
			}
		}

		private static SkiaCanvasState CreateNewState(object context)
		{
			if (_defaultFillPaint == null)
			{
				_defaultFillPaint = new SKPaint
				{
					Color = SKColors.White,
					IsStroke = false,
					IsAntialias = true
				};

				_defaultStrokePaint = new SKPaint
				{
					Color = SKColors.Black,
					StrokeWidth = 1,
					StrokeMiter = CanvasDefaults.DefaultMiterLimit,
					IsStroke = true,
					IsAntialias = true
				};

				_defaultFontPaint = new SKPaint
				{
					Color = SKColors.Black,
					IsAntialias = true,
					Typeface = SKTypeface.FromFamilyName("Arial")
				};
			}

			var state = new SkiaCanvasState
			{
				FillPaint = _defaultFillPaint.CreateCopy(),
				StrokePaint = _defaultStrokePaint.CreateCopy(),
				FontPaint = _defaultFontPaint.CreateCopy(),
				FontName = SkiaGraphicsService.Instance.SystemFontName
			};

			return state;
		}

		public void SetDisplayScale(float value)
		{
			_displayScale = value;
		}

		private static SkiaCanvasState CreateStateCopy(SkiaCanvasState prototype)
		{
			return new SkiaCanvasState(prototype);
		}

		public override void Dispose()
		{
			_defaultFillPaint.Dispose();
			_defaultStrokePaint.Dispose();
			_defaultFontPaint.Dispose();

			base.Dispose();
		}

		protected override void NativeSetStrokeDashPattern(
			float[] pattern,
			float strokeSize)
		{
			CurrentState.SetStrokeDashPattern(pattern, strokeSize);
		}

		public override void SetToSystemFont()
		{
			CurrentState.FontName = SkiaGraphicsService.Instance.SystemFontName;
		}

		public override void SetToBoldSystemFont()
		{
			CurrentState.FontName = SkiaGraphicsService.Instance.BoldSystemFontName;
		}

		public override void SetFillPaint(Paint paint, RectangleF rectangle)
		{
			if (paint == null)
				paint = Colors.White.AsPaint();

			if (_shader != null)
			{
				CurrentState.SetFillPaintShader(null);
				_shader.Dispose();
				_shader = null;
			}

			if (paint is LinearGradientPaint linearGradientPaint)
			{
				float x1 = (float)(linearGradientPaint.StartPoint.X * rectangle.Width) + rectangle.X;
				float y1 = (float)(linearGradientPaint.StartPoint.Y * rectangle.Height) + rectangle.Y;

				float x2 = (float)(linearGradientPaint.EndPoint.X * rectangle.Width) + rectangle.X;
				float y2 = (float)(linearGradientPaint.EndPoint.Y * rectangle.Height) + rectangle.Y;

				var colors = new SKColor[linearGradientPaint.GradientStops.Length];
				var stops = new float[colors.Length];

				var vStops = linearGradientPaint.GetSortedStops();

				for (var i = 0; i < vStops.Length; i++)
				{
					colors[i] = vStops[i].Color.ToColor(CurrentState.Alpha);
					stops[i] = vStops[i].Offset;
				}

				try
				{
					CurrentState.FillColor = Colors.White;
					_shader = SKShader.CreateLinearGradient(
						new SKPoint(x1, y1),
						new SKPoint(x2, y2),
						colors,
						stops,
						SKShaderTileMode.Clamp);
					CurrentState.SetFillPaintShader(_shader);
				}
				catch (Exception exc)
				{
					Logger.Debug(exc);
					FillColor = linearGradientPaint.BlendStartAndEndColors();
				}
			}
			else if (paint is RadialGradientPaint radialGradientPaint)
			{
				var colors = new SKColor[radialGradientPaint.GradientStops.Length];
				var stops = new float[colors.Length];

				var vStops = radialGradientPaint.GetSortedStops();

				for (var i = 0; i < vStops.Length; i++)
				{
					colors[i] = vStops[i].Color.ToColor(CurrentState.Alpha);
					stops[i] = vStops[i].Offset;
				}

				float centerX = (float)(radialGradientPaint.Center.X * rectangle.Width) + rectangle.X;
				float centerY = (float)(radialGradientPaint.Center.Y * rectangle.Height) + rectangle.Y;
				float radius = (float)radialGradientPaint.Radius * Math.Max(rectangle.Height, rectangle.Width);

				if (radius == 0)
					radius = Geometry.GetDistance(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

				try
				{
					CurrentState.FillColor = Colors.White;
					_shader = SKShader.CreateRadialGradient(
						new SKPoint(centerX, centerY),
						radius,
						colors,
						stops,
						SKShaderTileMode.Clamp);
					CurrentState.SetFillPaintShader(_shader);
				}
				catch (Exception exc)
				{
					Logger.Debug(exc);
					FillColor = radialGradientPaint.BlendStartAndEndColors();
				}
			}
			else if (paint is PatternPaint patternPaint)
			{
				SKBitmap bitmap = patternPaint.GetPatternBitmap(DisplayScale);

				if (bitmap != null)
				{
					try
					{
						CurrentState.FillColor = Colors.White;
						CurrentState.SetFillPaintFilterBitmap(true);

						_shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);

						//_shaderMatrix.Reset ();
						//_shaderMatrix.PreScale (CurrentState.ScaleX, CurrentState.ScaleY);
						//_shader.SetLocalMatrix (shaderMatrix);

						CurrentState.SetFillPaintShader(_shader);
					}
					catch (Exception exc)
					{
						Logger.Debug(exc);
						FillColor = paint.BackgroundColor;
					}
				}
				else
				{
					FillColor = paint.BackgroundColor;
				}
			}
			else if (paint is ImagePaint imagePaint)
			{
				var image = imagePaint.Image as SkiaImage;
				if (image != null)
				{
					SKBitmap bitmap = image.NativeImage;

					if (bitmap != null)
					{
						try
						{
							CurrentState.FillColor = Colors.White;
							CurrentState.SetFillPaintFilterBitmap(true);

							_shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
							//_shaderMatrix.Reset ();
							//_shaderMatrix.PreScale (CurrentState.ScaleX, CurrentState.ScaleY);
							//_shader.SetLocalMatrix (shaderMatrix);

							CurrentState.SetFillPaintShader(_shader);
						}
						catch (Exception exc)
						{
							Logger.Debug(exc);
							FillColor = paint.BackgroundColor;
						}
					}
					else
					{
						FillColor = Colors.White;
					}
				}
				else
				{
					FillColor = Colors.White;
				}
			}
			else
			{
				FillColor = paint.BackgroundColor;
			}
		}

		protected override void NativeDrawLine(
			float x1,
			float y1,
			float x2,
			float y2)
		{
			_canvas.DrawLine(x1, y1, x2, y2, CurrentState.StrokePaintWithAlpha);
		}

		protected override void NativeDrawArc(
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float endAngle,
			bool clockwise,
			bool closed)
		{
			while (startAngle < 0)
				startAngle += 360;

			while (endAngle < 0)
				endAngle += 360;

			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			var sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);

			var rect = new SKRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight);

			startAngle *= -1;
			if (!clockwise)
				sweep *= -1;

			if (closed)
			{
				var nativePath = new SKPath();
				nativePath.AddArc(rect, startAngle, sweep);
				nativePath.Close();
				_canvas.DrawPath(nativePath, CurrentState.StrokePaintWithAlpha);
				nativePath.Dispose();
			}
			else
			{
				// todo: delete this after the api is bound
				var nativePath = new SKPath();
				nativePath.AddArc(rect, startAngle, sweep);
				_canvas.DrawPath(nativePath, CurrentState.StrokePaintWithAlpha);
				nativePath.Dispose();

				// todo: restore this when the api is bound.
				//_canvas.DrawArc (rect, startAngle, sweep, false, CurrentState.StrokePaintWithAlpha);
			}
		}

		public override void FillArc(
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float endAngle,
			bool clockwise)
		{
			while (startAngle < 0)
				startAngle += 360;

			while (endAngle < 0)
				endAngle += 360;

			var sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);
			var rect = new SKRect(x, y, x + width, y + height);

			startAngle *= -1;
			if (!clockwise)
				sweep *= -1;

			// todo: delete this after the api is bound
			var nativePath = new SKPath();
			nativePath.AddArc(rect, startAngle, sweep);
			_canvas.DrawPath(nativePath, CurrentState.FillPaintWithAlpha);
			nativePath.Dispose();

			// todo: restore this when the api is bound.
			//_canvas.DrawArc (rect, startAngle, sweep, false, CurrentState.FillPaintWithAlpha);
		}

		protected override void NativeDrawRectangle(
			float x,
			float y,
			float width,
			float height)
		{
			float rectX = 0, rectY = 0, rectWidth = 0, rectHeight = 0;

			var strokeSize = CurrentState.ScaledStrokeSize;
			if (strokeSize <= 0)
				return;

			rectX = x;
			rectY = y;
			rectWidth = width;
			rectHeight = height;

			_canvas.DrawRect(rectX, rectY, rectWidth, rectHeight, CurrentState.StrokePaintWithAlpha);
		}

		public override void FillRectangle(
			float x,
			float y,
			float width,
			float height)
		{
			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			_canvas.DrawRect(rectX, rectY, rectWidth, rectHeight, CurrentState.FillPaintWithAlpha);
		}

		protected override void NativeDrawRoundedRectangle(
			float x,
			float y,
			float width,
			float height,
			float aCornerRadius)
		{
			// These values work for a stroke location of center.
			var strokeSize = CurrentState.ScaledStrokeSize;

			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;
			var radius = aCornerRadius;

			_canvas.DrawRoundRect(rectX, rectY, rectWidth, rectHeight, radius, radius, CurrentState.StrokePaintWithAlpha);
		}

		public override void FillRoundedRectangle(
			float x,
			float y,
			float width,
			float height,
			float aCornerRadius)
		{
			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;
			var radius = aCornerRadius;

			var rect = new SKRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight);
			_canvas.DrawRoundRect(rect, radius, radius, CurrentState.FillPaintWithAlpha);
		}

		protected override void NativeDrawEllipse(
			float x,
			float y,
			float width,
			float height)
		{
			// These values work for a stroke location of center.
			var strokeSize = CurrentState.ScaledStrokeSize;

			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			var rect = new SKRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight);
			_canvas.DrawOval(rect, CurrentState.StrokePaintWithAlpha);
		}

		public override void FillEllipse(
			float x,
			float y,
			float width,
			float height)
		{
			// These values work for a stroke location of center.
			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			var rect = new SKRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight);
			_canvas.DrawOval(rect, CurrentState.FillPaintWithAlpha);
		}

		public override void SubtractFromClip(
			float x,
			float y,
			float width,
			float height)
		{
			var rect = new SKRect(x, y, x + width, y + height);
			_canvas.ClipRect(rect, SKClipOperation.Difference);
		}

		protected override void NativeDrawPath(
			PathF path)
		{
			var nativePath = path.AsSkiaPath();
			_canvas.DrawPath(nativePath, CurrentState.StrokePaintWithAlpha);
			nativePath.Dispose();
		}

		public override void ClipPath(PathF path,
			WindingMode windingMode = WindingMode.NonZero)
		{
			var nativePath = path.AsSkiaPath();
			nativePath.FillType = windingMode == WindingMode.NonZero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
			_canvas.ClipPath(nativePath);
		}

		public override void FillPath(PathF path,
			WindingMode windingMode)
		{
			var nativePath = path.AsSkiaPath();
			nativePath.FillType = windingMode == WindingMode.NonZero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
			_canvas.DrawPath(nativePath, CurrentState.FillPaintWithAlpha);
			nativePath.Dispose();
		}

		public override void DrawString(
			string value,
			float x,
			float y,
			HorizontalAlignment horizAlignment)
		{
			if (string.IsNullOrEmpty(value))
				return;

			if (horizAlignment == HorizontalAlignment.Left)
			{
				_canvas.DrawText(value, x, y, CurrentState.FontPaint);
			}
			else if (horizAlignment == HorizontalAlignment.Right)
			{
				var paint = CurrentState.FontPaint;
				var width = paint.MeasureText(value);
				x -= width;
				_canvas.DrawText(value, x, y, CurrentState.FontPaint);
			}
			else
			{
				var paint = CurrentState.FontPaint;
				var width = paint.MeasureText(value);
				x -= width / 2;
				_canvas.DrawText(value, x, y, CurrentState.FontPaint);
			}
		}

		public override void DrawString(
			string value,
			float x,
			float y,
			float width,
			float height,
			HorizontalAlignment horizAlignment,
			VerticalAlignment vertAlignment,
			TextFlow textFlow = TextFlow.ClipBounds,
			float lineSpacingAdjustment = 0)
		{
			if (string.IsNullOrEmpty(value))
				return;

			var rect = new RectangleF(x, y, width, height);

			var attributes = new StandardTextAttributes()
			{
				FontSize = CurrentState.FontPaint.TextSize,
				FontName = CurrentState.FontName,
				HorizontalAlignment = horizAlignment,
				VerticalAlignment = vertAlignment,
			};

			LayoutLine callback = (
				point,
				textual,
				text,
				ascent,
				descent,
				leading) =>
			{
				_canvas.DrawText(text, point.X, point.Y, CurrentState.FontPaint);
			};

			using (var textLayout = new SkiaTextLayout(value, rect, attributes, callback, textFlow, CurrentState.FontPaint))
			{
				textLayout.LayoutText();
			}
		}

		public override void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			Logger.Debug("Not yet implemented.");
			DrawString(value?.Text, x, y, width, height, HorizontalAlignment.Left, VerticalAlignment.Top);
		}

		public override void ResetState()
		{
			base.ResetState();

			if (_shader != null)
			{
				_shader.Dispose();
				_shader = null;
			}

			CurrentState.Reset(_defaultFontPaint, _defaultFillPaint, _defaultStrokePaint);
		}

		public override bool RestoreState()
		{
			if (_shader != null)
			{
				_shader.Dispose();
				_shader = null;
			}

			return base.RestoreState();
		}

		protected override void StateRestored(SkiaCanvasState state)
		{
			_canvas?.Restore();
		}

		public override void SetShadow(
			SizeF offset,
			float blur,
			Color color)
		{
			var actualOffset = offset;
			if (actualOffset == null)
				actualOffset = CanvasDefaults.DefaultShadowOffset;

			var sx = actualOffset.Width;
			var sy = actualOffset.Height;

			if (color == null)
			{
				var actualColor = Colors.Black.AsSKColorMultiplyAlpha(CurrentState.Alpha);
				CurrentState.SetShadow(blur, sx, sy, actualColor);
			}
			else
			{
				var actualColor = color.AsSKColorMultiplyAlpha(CurrentState.Alpha);
				CurrentState.SetShadow(blur, sx, sy, actualColor);
			}
		}

		protected override void NativeRotate(
			float degrees,
			float radians,
			float x,
			float y)
		{
			_canvas.RotateDegrees(degrees, x, y);
		}

		protected override void NativeRotate(
			float degrees,
			float radians)
		{
			_canvas.RotateDegrees(degrees);
		}

		protected override void NativeScale(
			float xFactor,
			float yFactor)
		{
			//canvas.Scale((float)aXFactor, (float)aYFactor);
			CurrentState.SetScale(Math.Abs(xFactor), Math.Abs(yFactor));
			if (xFactor < 0 || yFactor < 0)
				_canvas.Scale(xFactor < 0 ? -1 : 1, yFactor < 0 ? -1 : 1);
		}

		protected override void NativeTranslate(
			float tx,
			float ty)
		{
			_canvas.Translate(tx * CurrentState.ScaleX, ty * CurrentState.ScaleY);
		}

		protected override void NativeConcatenateTransform(AffineTransform transform)
		{
			var matrix = new SKMatrix();

			var values = new float[6];
			_canvas.TotalMatrix.GetValues(values);
			matrix.Values = values;

			// todo: implement me
			//matrix.PostConcat (transform.AsMatrix ());
			//canvas.Matrix = matrix;
		}

		public override void SaveState()
		{
			_canvas.Save();
			base.SaveState();
		}

		public void SetBlur(float radius)
		{
			CurrentState.SetBlur(radius);
		}

		public override void DrawImage(
			IImage image,
			float x,
			float y,
			float width,
			float height)
		{
			var skiaImage = image as SkiaImage;
			var bitmap = skiaImage?.NativeImage;
			if (bitmap != null)
			{
				var scaleX = CurrentState.ScaleX < 0 ? -1 : 1;
				var scaleY = CurrentState.ScaleY < 0 ? -1 : 1;

				_canvas.Save();
				//canvas.Scale (scaleX, scaleY);
				var srcRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);

				x *= scaleX;
				y *= scaleY;
				width *= scaleX;
				height *= scaleY;

				var rx1 = Math.Min(x, x + width);
				var ry1 = Math.Min(y, y + height);
				var rx2 = Math.Max(x, x + width);
				var ry2 = Math.Max(y, y + height);

				var destRect = new SKRect(rx1, ry1, rx2, ry2);
				var paint = CurrentState.GetImagePaint(1, 1);
				_canvas.DrawBitmap(bitmap, srcRect, destRect, paint);
				paint?.Dispose();
				_canvas.Restore();
			}
		}

		public override void ClipRectangle(
			float x,
			float y,
			float width,
			float height)
		{
			_canvas.ClipRect(new SKRect(x, y, x + width, y + height));
		}
	}
}
