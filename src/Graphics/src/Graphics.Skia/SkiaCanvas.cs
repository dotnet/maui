using System;
using System.Numerics;

using Microsoft.Maui.Graphics.Text;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Implements a canvas that uses SkiaSharp for rendering graphics.
	/// </summary>
	/// <remarks>
	/// SkiaCanvas provides a implementation of the standard canvas drawing operations using the SkiaSharp graphics library.
	/// </remarks>
	public class SkiaCanvas : AbstractCanvas<SkiaCanvasState>, IBlurrableCanvas
	{
		private readonly SkiaCanvasStateService _stateService;

		private SKCanvas _canvas;
		private float _displayScale = 1;
		private SKShader _shader;

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaCanvas"/> class.
		/// </summary>
		public SkiaCanvas()
			: base(CreateStateService(out var stateService), new SkiaStringSizeService())
		{
			_stateService = stateService;
		}

		static SkiaCanvasStateService CreateStateService(out SkiaCanvasStateService stateService) =>
			stateService = new SkiaCanvasStateService();

		/// <summary>
		/// Releases all resources used by the <see cref="SkiaCanvas"/> instance.
		/// </summary>
		public override void Dispose()
		{
			_stateService.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// Gets the display scale factor used by the canvas.
		/// </summary>
		public override float DisplayScale => _displayScale;

		/// <summary>
		/// Gets or sets the underlying SkiaSharp canvas.
		/// </summary>
		/// <remarks>
		/// Setting a new canvas will reset the current state.
		/// </remarks>
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

		/// <summary>
		/// Gets or sets a value indicating whether anti-aliasing is enabled.
		/// </summary>
		public override bool Antialias
		{
			set => CurrentState.AntiAlias = value;
		}

		protected override float PlatformStrokeSize
		{
			set => CurrentState.PlatformStrokeSize = value;
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

		public override IFont Font
		{
			set => CurrentState.Font = value;
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

		/// <summary>
		/// Sets the display scale factor for the canvas.
		/// </summary>
		/// <param name="value">The display scale factor to set.</param>
		public void SetDisplayScale(float value)
		{
			_displayScale = value;
		}

		protected override void PlatformSetStrokeDashPattern(
			float[] strokePattern,
			float strokeDashOffset,
			float strokeSize)
		{
			CurrentState.SetStrokeDashPattern(strokePattern, strokeDashOffset, strokeSize);
		}

		/// <summary>
		/// Sets the fill paint for the canvas.
		/// </summary>
		/// <param name="paint">The paint to use for filling. If null, white color will be used.</param>
		/// <param name="rectangle">The rectangle that defines the coordinate space for the paint.</param>
		/// <remarks>
		/// This method handles different types of paints including SolidPaint, LinearGradientPaint, 
		/// RadialGradientPaint, PatternPaint, and ImagePaint.
		/// </remarks>
		public override void SetFillPaint(Paint paint, RectF rectangle)
		{
			if (paint == null)
				paint = Colors.White.AsPaint();

			if (_shader != null)
			{
				CurrentState.SetFillPaintShader(null);
				_shader.Dispose();
				_shader = null;
			}

			if (paint is SolidPaint solidPaint)
			{
				FillColor = solidPaint.Color;
			}
			else if (paint is LinearGradientPaint linearGradientPaint)
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
					System.Diagnostics.Debug.WriteLine(exc);
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
					radius = GeometryUtil.GetDistance(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

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
					System.Diagnostics.Debug.WriteLine(exc);
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
						System.Diagnostics.Debug.WriteLine(exc);
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
					SKBitmap bitmap = image.PlatformRepresentation;

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
							System.Diagnostics.Debug.WriteLine(exc);
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

		protected override void PlatformDrawLine(
			float x1,
			float y1,
			float x2,
			float y2)
		{
			_canvas.DrawLine(x1, y1, x2, y2, CurrentState.StrokePaintWithAlpha);
		}

		protected override void PlatformDrawArc(
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

			var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);

			var rect = new SKRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight);

			startAngle *= -1;
			if (!clockwise)
				sweep *= -1;

			if (closed)
			{
				var platformPath = new SKPath();
				platformPath.AddArc(rect, startAngle, sweep);
				platformPath.Close();
				_canvas.DrawPath(platformPath, CurrentState.StrokePaintWithAlpha);
				platformPath.Dispose();
			}
			else
			{
				// todo: delete this after the api is bound
				var platformPath = new SKPath();
				platformPath.AddArc(rect, startAngle, sweep);
				_canvas.DrawPath(platformPath, CurrentState.StrokePaintWithAlpha);
				platformPath.Dispose();

				// todo: restore this when the api is bound.
				//_canvas.DrawArc (rect, startAngle, sweep, false, CurrentState.StrokePaintWithAlpha);
			}
		}

		/// <summary>
		/// Fills an arc within the specified rectangle from the start angle to the end angle.
		/// </summary>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangle that contains the arc.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangle that contains the arc.</param>
		/// <param name="width">The width of the rectangle that contains the arc.</param>
		/// <param name="height">The height of the rectangle that contains the arc.</param>
		/// <param name="startAngle">The starting angle in degrees.</param>
		/// <param name="endAngle">The ending angle in degrees.</param>
		/// <param name="clockwise">A value indicating whether the arc is drawn clockwise or counterclockwise.</param>
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

			var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);
			var rect = new SKRect(x, y, x + width, y + height);

			startAngle *= -1;
			if (!clockwise)
				sweep *= -1;

			// todo: delete this after the api is bound
			var platformPath = new SKPath();
			platformPath.AddArc(rect, startAngle, sweep);
			_canvas.DrawPath(platformPath, CurrentState.FillPaintWithAlpha);
			platformPath.Dispose();

			// todo: restore this when the api is bound.
			//_canvas.DrawArc (rect, startAngle, sweep, false, CurrentState.FillPaintWithAlpha);
		}

		protected override void PlatformDrawRectangle(
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

		/// <summary>
		/// Fills a rectangle at the specified coordinates with the current fill paint.
		/// </summary>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
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

		protected override void PlatformDrawRoundedRectangle(
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

		protected override void PlatformDrawEllipse(
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

		protected override void PlatformDrawPath(
			PathF path)
		{
			var platformPath = path.AsSkiaPath();
			_canvas.DrawPath(platformPath, CurrentState.StrokePaintWithAlpha);
			platformPath.Dispose();
		}

		public override void ClipPath(PathF path,
			WindingMode windingMode = WindingMode.NonZero)
		{
			var platformPath = path.AsSkiaPath();
			platformPath.FillType = windingMode == WindingMode.NonZero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
			_canvas.ClipPath(platformPath);
		}

		public override void FillPath(PathF path,
			WindingMode windingMode)
		{
			var platformPath = path.AsSkiaPath();
			platformPath.FillType = windingMode == WindingMode.NonZero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;
			_canvas.DrawPath(platformPath, CurrentState.FillPaintWithAlpha);
			platformPath.Dispose();
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
				_canvas.DrawText(value, x, y, CurrentState.FontFont, CurrentState.FontPaint);
			}
			else if (horizAlignment == HorizontalAlignment.Right)
			{
				var font = CurrentState.FontFont;
				var width = font.MeasureText(value);
				x -= width;
				_canvas.DrawText(value, x, y, CurrentState.FontFont, CurrentState.FontPaint);
			}
			else
			{
				var font = CurrentState.FontFont;
				var width = font.MeasureText(value);
				x -= width / 2;
				_canvas.DrawText(value, x, y, CurrentState.FontFont, CurrentState.FontPaint);
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

			var rect = new RectF(x, y, width, height);

			var attributes = new StandardTextAttributes()
			{
				FontSize = CurrentState.FontFont.Size,
				Font = CurrentState.Font,
				HorizontalAlignment = horizAlignment,
				VerticalAlignment = vertAlignment,
			};

			var align = horizAlignment switch
			{
				HorizontalAlignment.Left => SKTextAlign.Left,
				HorizontalAlignment.Center => SKTextAlign.Center,
				HorizontalAlignment.Right => SKTextAlign.Right,
				_ => SKTextAlign.Left,
			};

			void DrawLineCallback(PointF point, ITextAttributes textual, string text, float ascent, float descent, float leading)
			{
				_canvas.DrawText(text, point.X, point.Y, align, CurrentState.FontFont, CurrentState.FontPaint);
			}

			using var textLayout = new SkiaTextLayout(value, rect, attributes, DrawLineCallback, textFlow, CurrentState.FontFont);
			textLayout.LayoutText();
		}

		public override void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			System.Diagnostics.Debug.WriteLine("SkiaCanvas.DrawText not yet implemented.");
			DrawString(value?.Text, x, y, width, height, HorizontalAlignment.Left, VerticalAlignment.Top);
		}

		public override void ResetState()
		{
			base.ResetState();

			_shader?.Dispose();
			_shader = null;

			_stateService.Reset(CurrentState);
		}

		public override bool RestoreState()
		{
			_shader?.Dispose();
			_shader = null;

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

		protected override void PlatformRotate(
			float degrees,
			float radians,
			float x,
			float y)
		{
			_canvas.RotateDegrees(degrees, x, y);
		}

		protected override void PlatformRotate(
			float degrees,
			float radians)
		{
			_canvas.RotateDegrees(degrees);
		}

		protected override void PlatformScale(
			float xFactor,
			float yFactor)
		{
			//canvas.Scale((float)aXFactor, (float)aYFactor);
			CurrentState.SetScale(Math.Abs(xFactor), Math.Abs(yFactor));
			if (xFactor < 0 || yFactor < 0)
				_canvas.Scale(xFactor < 0 ? -1 : 1, yFactor < 0 ? -1 : 1);
		}

		protected override void PlatformTranslate(
			float tx,
			float ty)
		{
			_canvas.Translate(tx * CurrentState.ScaleX, ty * CurrentState.ScaleY);
		}

		protected override void PlatformConcatenateTransform(Matrix3x2 transform)
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
			var bitmap = skiaImage?.PlatformRepresentation;
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
