using Microsoft.Maui.Graphics.Native.Text;
using Microsoft.Maui.Graphics.Text;
using Android.Content;
using Android.Graphics;
using Android.Text;
using System;
using System.Numerics;

namespace Microsoft.Maui.Graphics.Native
{
	public class NativeCanvas : AbstractCanvas<NativeCanvasState>
	{
		private static global::Android.Graphics.Paint _defaultFillPaint;
		private static TextPaint _defaultFontPaint;
		private static global::Android.Graphics.Paint _defaultStrokePaint;

		private Canvas _canvas;
		private Shader _shader;

		private readonly Matrix _shaderMatrix = new Matrix();

		public NativeCanvas(Context context) : base(CreateNewState, CreateStateCopy)
		{
			DisplayScale = context?.Resources?.DisplayMetrics?.Density ?? 1;
		}

		private static NativeCanvasState CreateNewState(object context)
		{
			if (_defaultFillPaint == null)
			{
				_defaultFillPaint = new global::Android.Graphics.Paint();
				_defaultFillPaint.SetARGB(255, 255, 255, 255);
				_defaultFillPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
				_defaultFillPaint.AntiAlias = true;

				_defaultStrokePaint = new global::Android.Graphics.Paint();
				_defaultStrokePaint.SetARGB(255, 0, 0, 0);
				_defaultStrokePaint.StrokeWidth = 1;
				_defaultStrokePaint.StrokeMiter = CanvasDefaults.DefaultMiterLimit;
				_defaultStrokePaint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
				_defaultStrokePaint.AntiAlias = true;

				_defaultFontPaint = new TextPaint();
				_defaultFontPaint.SetARGB(255, 0, 0, 0);
				_defaultFontPaint.AntiAlias = true;

				var defaultFont = Typeface.Default;
				if (defaultFont != null)
					_defaultFontPaint.SetTypeface(defaultFont);
				else
					Logger.Warn("Unable to set the default font paint to Default");
			}

			var state = new NativeCanvasState
			{
				FillPaint = new global::Android.Graphics.Paint(_defaultFillPaint),
				StrokePaint = new global::Android.Graphics.Paint(_defaultStrokePaint),
				FontPaint = new TextPaint(_defaultFontPaint),
				FontName = NativeFontService.SystemFont
			};

			return state;
		}

		private static NativeCanvasState CreateStateCopy(NativeCanvasState prototype)
		{
			return new NativeCanvasState(prototype);
		}

		public override void Dispose()
		{
			_defaultFillPaint.Dispose();
			_defaultStrokePaint.Dispose();
			_defaultFontPaint.Dispose();

			base.Dispose();
		}

		public Canvas Canvas
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

		public override String FontName
		{
			set => CurrentState.FontName = value ?? NativeFontService.SystemFont;
		}

		public override float FontSize
		{
			set => CurrentState.FontSize = value;
		}

		public override Color FillColor
		{
			set => CurrentState.FillColor = value ?? Colors.White;
		}

		public override BlendMode BlendMode
		{
			set
			{
				/* todo: implement this
				CGBlendMode vBlendMode = CGBlendMode.Normal;

				switch (value)
				{
					case BlendMode.Clear:
						vBlendMode = CGBlendMode.Clear;
						break;
					case BlendMode.Color:
						vBlendMode = CGBlendMode.Color;
						break;
					case BlendMode.ColorBurn:
						vBlendMode = CGBlendMode.ColorBurn;
						break;
					case BlendMode.ColorDodge:
						vBlendMode = CGBlendMode.ColorDodge;
						break;
					case BlendMode.Copy:
						vBlendMode = CGBlendMode.Copy;
						break;
					case BlendMode.Darken:
						vBlendMode = CGBlendMode.Darken;
						break;
					case BlendMode.DestinationAtop:
						vBlendMode = CGBlendMode.DestinationAtop;
						break;
					case BlendMode.DestinationIn:
						vBlendMode = CGBlendMode.DestinationIn;
						break;
					case BlendMode.DestinationOut:
						vBlendMode = CGBlendMode.DestinationOut;
						break;
					case BlendMode.DestinationOver:
						vBlendMode = CGBlendMode.DestinationOver;
						break;
					case BlendMode.Difference:
						vBlendMode = CGBlendMode.Difference;
						break;
					case BlendMode.Exclusion:
						vBlendMode = CGBlendMode.Exclusion;
						break;
					case BlendMode.HardLight:
						vBlendMode = CGBlendMode.HardLight;
						break;
					case BlendMode.Hue:
						vBlendMode = CGBlendMode.Hue;
						break;
					case BlendMode.Lighten:
						vBlendMode = CGBlendMode.Lighten;
						break;
					case BlendMode.Luminosity:
						vBlendMode = CGBlendMode.Luminosity;
						break;
					case BlendMode.Multiply:
						vBlendMode = CGBlendMode.Multiply;
						break;
					case BlendMode.Normal:
						vBlendMode = CGBlendMode.Normal;
						break;
					case BlendMode.Overlay:
						vBlendMode = CGBlendMode.Overlay;
						break;
					case BlendMode.PlusDarker:
						vBlendMode = CGBlendMode.PlusDarker;
						break;
					case BlendMode.PlusLighter:
						vBlendMode = CGBlendMode.PlusLighter;
						break;
					case BlendMode.Saturation:
						vBlendMode = CGBlendMode.Saturation;
						break;
					case BlendMode.Screen:
						vBlendMode = CGBlendMode.Screen;
						break;
					case BlendMode.SoftLight:
						vBlendMode = CGBlendMode.SoftLight;
						break;
					case BlendMode.SourceAtop:
						vBlendMode = CGBlendMode.SourceAtop;
						break;
					case BlendMode.SourceIn:
						vBlendMode = CGBlendMode.SourceIn;
						break;
					case BlendMode.SourceOut:
						vBlendMode = CGBlendMode.SourceOut;
						break;
					case BlendMode.XOR:
						vBlendMode = CGBlendMode.XOR;
						break;
				}

				canvas.SetBlendMode(vBlendMode);*/

				//CurrentState.FillPaint.SetXfermode(new
			}
		}

		protected override void NativeSetStrokeDashPattern(float[] patter, float linewidth)
		{
			CurrentState.SetStrokeDashPattern(patter, linewidth);
		}

		public override void SetToSystemFont()
		{
			CurrentState.FontName = NativeFontService.SystemFont;
		}

		public override void SetToBoldSystemFont()
		{
			CurrentState.FontName = NativeFontService.SystemBoldFont;
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

			if (paint is SolidPaint solidPaint)
			{
				FillColor = solidPaint.Color;
			}
			else if (paint is LinearGradientPaint linearGradientPaint)
			{
				var colors = new int[linearGradientPaint.GradientStops.Length];
				var stops = new float[colors.Length];

				GradientStop[] vStops = linearGradientPaint.GetSortedStops();

				for (int i = 0; i < vStops.Length; i++)
				{
					colors[i] = vStops[i].Color.MultiplyAlpha(CurrentState.Alpha).ToInt();
					stops[i] = vStops[i].Offset;
				}

				try
				{
					CurrentState.FillColor = Colors.White;

					float x1 = (float)(linearGradientPaint.StartPoint.X * rectangle.Width) + rectangle.X;
					float y1 = (float)(linearGradientPaint.StartPoint.Y * rectangle.Height) + rectangle.Y;

					float x2 = (float)(linearGradientPaint.EndPoint.X * rectangle.Width) + rectangle.X;
					float y2 = (float)(linearGradientPaint.EndPoint.Y * rectangle.Height) + rectangle.Y;

					_shader = new LinearGradient(x1, y1, x2, y2, colors, stops, Shader.TileMode.Clamp);
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
				var colors = new int[radialGradientPaint.GradientStops.Length];
				var stops = new float[colors.Length];

				GradientStop[] vStops = radialGradientPaint.GetSortedStops();

				for (int i = 0; i < vStops.Length; i++)
				{
					colors[i] = vStops[i].Color.MultiplyAlpha(CurrentState.Alpha).ToInt();
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
					_shader = new RadialGradient(centerX, centerY, radius, colors, stops, Shader.TileMode.Clamp);
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
				var bitmap = patternPaint.GetPatternBitmap(DisplayScale);

				if (bitmap != null)
				{
					try
					{
						CurrentState.FillColor = Colors.White;
						CurrentState.SetFillPaintFilterBitmap(true);

						_shader = new BitmapShader(bitmap, Shader.TileMode.Repeat, Shader.TileMode.Repeat);
						_shaderMatrix.Reset();
						_shaderMatrix.PreScale(CurrentState.ScaleX, CurrentState.ScaleY);
						_shader.SetLocalMatrix(_shaderMatrix);

						CurrentState.SetFillPaintShader(_shader);
					}
					catch (Exception exc)
					{
						Logger.Debug(exc);
						FillColor = patternPaint.BackgroundColor;
					}
				}
				else
				{
					FillColor = patternPaint.BackgroundColor;
				}
			}
			else if (paint is ImagePaint imagePaint)
			{
				if (imagePaint.Image is NativeImage image)
				{
					var bitmap = image.NativeRepresentation;

					if (bitmap != null)
					{
						try
						{
							CurrentState.FillColor = Colors.White;
							CurrentState.SetFillPaintFilterBitmap(true);

							_shader = new BitmapShader(bitmap, Shader.TileMode.Repeat, Shader.TileMode.Repeat);
							_shaderMatrix.Reset();
							_shaderMatrix.PreScale(CurrentState.ScaleX, CurrentState.ScaleY);
							_shader.SetLocalMatrix(_shaderMatrix);

							CurrentState.SetFillPaintShader(_shader);
						}
						catch (Exception exc)
						{
							Logger.Debug(exc);
							FillColor = paint.BackgroundColor;
						}
					}
					else
						FillColor = Colors.White;
				}
				else
				{
					FillColor = Colors.White;
				}
			}
			else
				FillColor = paint.BackgroundColor;

			//Logger.Debug("Gradient Set To: "+aPaint.PaintType);
		}

		protected override void NativeDrawLine(float x1, float y1, float x2, float y2)
		{
			//canvas.DrawLine (x1, y1, x2, y2, CurrentState.StrokePaintWithAlpha);

			var nativePath = new Path();
			nativePath.MoveTo(x1, y1);
			nativePath.LineTo(x2, y2);
			_canvas.DrawPath(nativePath, CurrentState.StrokePaintWithAlpha);
			nativePath.Dispose();
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
			{
				startAngle += 360;
			}

			while (endAngle < 0)
			{
				endAngle += 360;
			}

			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			float sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);

			var rect = new RectF(rectX, rectY, rectX + rectWidth, rectY + rectHeight);

			startAngle *= -1;
			if (!clockwise)
			{
				sweep *= -1;
			}

			if (closed)
			{
				var nativePath = new Path();
				nativePath.AddArc(rect, startAngle, sweep);
				nativePath.Close();
				_canvas.DrawPath(nativePath, CurrentState.StrokePaintWithAlpha);
				nativePath.Dispose();
			}
			else
			{
				_canvas.DrawArc(rect, startAngle, sweep, false, CurrentState.StrokePaintWithAlpha);
			}

			rect.Dispose();
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
			{
				startAngle += 360;
			}

			while (endAngle < 0)
			{
				endAngle += 360;
			}

			var sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);
			var rect = new RectF(x, y, x + width, y + height);

			startAngle *= -1;
			if (!clockwise)
			{
				sweep *= -1;
			}

			_canvas.DrawArc(rect, startAngle, sweep, false, CurrentState.FillPaintWithAlpha);
			rect.Dispose();
		}

		protected override void NativeDrawRectangle(float x, float y, float width, float height)
		{
			float rectX = 0, rectY = 0, rectWidth = 0, rectHeight = 0;

			float strokeSize = CurrentState.ScaledStrokeSize;
			if (strokeSize == 0)
				return;

			rectX = x;
			rectY = y;
			rectWidth = width;
			rectHeight = height;

			_canvas.DrawRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight, CurrentState.StrokePaintWithAlpha);
		}

		public override void FillRectangle(float x, float y, float width, float height)
		{
			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			_canvas.DrawRect(rectX, rectY, rectX + rectWidth, rectY + rectHeight, CurrentState.FillPaintWithAlpha);
		}

		protected override void NativeDrawRoundedRectangle(
			float x,
			float y,
			float width,
			float height,
			float aCornerRadius)
		{
			// These values work for a stroke location of center.
			float strokeSize = CurrentState.ScaledStrokeSize;

			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;
			var radius = aCornerRadius;

			var rect = new RectF(rectX, rectY, rectX + rectWidth, rectY + rectHeight);
			_canvas.DrawRoundRect(rect, radius, radius, CurrentState.StrokePaintWithAlpha);
			rect.Dispose();
		}

		public override void FillRoundedRectangle(float x, float y, float width, float height, float aCornerRadius)
		{
			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;
			var radius = aCornerRadius;

			var rect = new RectF(rectX, rectY, rectX + rectWidth, rectY + rectHeight);
			_canvas.DrawRoundRect(rect, radius, radius, CurrentState.FillPaintWithAlpha);
			rect.Dispose();
		}

		protected override void NativeDrawEllipse(float x, float y, float width, float height)
		{
			// These values work for a stroke location of center.
			float strokeSize = CurrentState.ScaledStrokeSize;

			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			var rect = new RectF(rectX, rectY, rectX + rectWidth, rectY + rectHeight);
			_canvas.DrawOval(rect, CurrentState.StrokePaintWithAlpha);
			rect.Dispose();
		}

		public override void FillEllipse(float x, float y, float width, float height)
		{
			/* todo: support gradients here */

			// These values work for a stroke location of center.
			var rectX = x;
			var rectY = y;
			var rectWidth = width;
			var rectHeight = height;

			var rect = new RectF(rectX, rectY, rectX + rectWidth, rectY + rectHeight);
			_canvas.DrawOval(rect, CurrentState.FillPaintWithAlpha);
			rect.Dispose();
		}

		public override void SubtractFromClip(float x, float y, float width, float height)
		{
			_canvas.ClipRect(x, y, x + width, y + height, Region.Op.Difference);
		}

		protected override void NativeDrawPath(PathF aPath)
		{
			var nativePath = aPath.AsAndroidPath();
			_canvas.DrawPath(nativePath, CurrentState.StrokePaintWithAlpha);
			nativePath.Dispose();
		}

		public override void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
		{
			var nativePath = path.AsAndroidPath();
			nativePath.SetFillType(windingMode == WindingMode.NonZero ? Path.FillType.Winding : Path.FillType.EvenOdd);
			_canvas.ClipPath(nativePath);
		}

		public override void ClipRectangle(float x, float y, float width, float height)
		{
			_canvas.ClipRect(x, y, x + width, y + height);
		}

		public override void FillPath(PathF path, WindingMode windingMode)
		{
			var nativePath = path.AsAndroidPath();
			nativePath.SetFillType(windingMode == WindingMode.NonZero ? Path.FillType.Winding : Path.FillType.EvenOdd);
			_canvas.DrawPath(nativePath, CurrentState.FillPaintWithAlpha);
			nativePath.Dispose();
		}

		public override void DrawString(string value, float x, float y, HorizontalAlignment horizAlignment)
		{
			if (horizAlignment == HorizontalAlignment.Left)
				DrawString(value, x, y);
			else if (horizAlignment == HorizontalAlignment.Right)
			{
				SizeF vSize = NativeGraphicsService.Instance.GetStringSize(
					value,
					CurrentState.FontName,
					CurrentState.ScaledFontSize);
				x -= vSize.Width;
				DrawString(value, x, y);
			}
			else
			{
				SizeF vSize = NativeGraphicsService.Instance.GetStringSize(
					value,
					CurrentState.FontName,
					CurrentState.ScaledFontSize);
				x -= vSize.Width / 2f;
				DrawString(value, x, y);
			}
		}

		private void DrawString(string value, float x, float y)
		{
			if (value == null)
				return;

			_canvas.Save();
			_canvas.Translate(x, y - CurrentState.ScaledFontSize);
			var layout = new StaticLayout(
				value,
				CurrentState.FontPaint,
				512,
				Layout.Alignment.AlignNormal,
				1f,
				0f,
				false);
			layout.Draw(_canvas);
			_canvas.Restore();
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
			if (value == null || value.Length == 0 || width == 0 || height == 0)
				return;

			_canvas.Save();

			var alignment = Layout.Alignment.AlignNormal;
			if (horizAlignment == HorizontalAlignment.Center)
			{
				alignment = Layout.Alignment.AlignCenter;
			}
			else if (horizAlignment == HorizontalAlignment.Right)
			{
				alignment = Layout.Alignment.AlignOpposite;
			}

			var layout = TextLayoutUtils.CreateLayout(value, CurrentState.FontPaint, (int) width, alignment);
			var offset = layout.GetOffsetsToDrawText(x, y, width, height, horizAlignment, vertAlignment);
			_canvas.Translate(offset.Width, offset.Height);
			layout.Draw(_canvas);
			_canvas.Restore();
		}

		public override void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			_canvas.Save();
			var span = value.AsSpannableString();
			var layout = TextLayoutUtils.CreateLayoutForSpannedString(span, CurrentState.FontPaint, (int) width, Layout.Alignment.AlignNormal);
			var offset = layout.GetOffsetsToDrawText(x, y, width, height, HorizontalAlignment.Left, VerticalAlignment.Top);
			_canvas.Translate(offset.Width, offset.Height);
			layout.Draw(_canvas);
			layout.Dispose();
			span.Dispose();
			_canvas.Restore();
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

		protected override void StateRestored(NativeCanvasState state)
		{
			_canvas?.Restore();
		}

		public override void SetShadow(SizeF offset, float blur, Color color)
		{
			SizeF actualOffset = offset;
			var sx = actualOffset.Width;
			var sy = actualOffset.Height;

			if (color == null)
			{
				var actualColor = Colors.Black.AsColorMultiplyAlpha(CurrentState.Alpha);
				CurrentState.SetShadow(blur, sx, sy, actualColor);
			}
			else
			{
				var actualColor = color.AsColorMultiplyAlpha(CurrentState.Alpha);
				CurrentState.SetShadow(blur, sx, sy, actualColor);
			}
		}

		protected override void NativeRotate(float degrees, float radians, float x, float y)
		{
			_canvas.Rotate(degrees, x, y);
		}

		protected override void NativeRotate(float degrees, float radians)
		{
			_canvas.Rotate(degrees);
		}

		protected override void NativeScale(float xFactor, float yFactor)
		{
			CurrentState.SetScale(Math.Abs(xFactor), Math.Abs(yFactor));
			if (xFactor < 0 || yFactor < 0)
				_canvas.Scale(xFactor < 0 ? -1 : 1, yFactor < 0 ? -1 : 1);
		}

		protected override void NativeTranslate(float tx, float ty)
		{
			_canvas.Translate(tx * CurrentState.ScaleX, ty * CurrentState.ScaleY);
		}

		protected override void NativeConcatenateTransform(Matrix3x2 transform)
		{
			var matrix = new Matrix(_canvas.Matrix);
			matrix.PostConcat(transform.AsMatrix());
			_canvas.Matrix = matrix;
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

		public override void DrawImage(IImage image, float x, float y, float width, float height)
		{
			if (image is NativeImage mdimage)
			{
				var bitmap = mdimage.NativeRepresentation;
				if (bitmap != null)
				{
					var scaleX = CurrentState.ScaleX < 0 ? -1 : 1;
					var scaleY = CurrentState.ScaleY < 0 ? -1 : 1;

					_canvas.Save();
					//canvas.Scale (scaleX, scaleY);
					var srcRect = new Rect(0, 0, bitmap.Width, bitmap.Height);

					x *= scaleX;
					y *= scaleY;
					width *= scaleX;
					height *= scaleY;

					var rx1 = Math.Min(x, x + width);
					var ry1 = Math.Min(y, y + height);
					var rx2 = Math.Max(x, x + width);
					var ry2 = Math.Max(y, y + height);

					var destRect = new RectF(rx1, ry1, rx2, ry2);
					var paint = CurrentState.GetImagePaint(1, 1);
					_canvas.DrawBitmap(bitmap, srcRect, destRect, paint);
					paint?.Dispose();
					_canvas.Restore();
				}
			}
		}
	}
}
