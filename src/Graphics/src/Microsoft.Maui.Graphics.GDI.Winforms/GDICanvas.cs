using System;
using System.Drawing;
using Drawing = System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Microsoft.Maui.Graphics.Text;
using System.Numerics;
using System.Windows.Forms;

namespace Microsoft.Maui.Graphics.GDI
{
	public class GDICanvas : AbstractCanvas<GDICanvasState>
	{
		private System.Drawing.Graphics _graphics;

		private Drawing.RectangleF _rect;
		private global::System.Drawing.Rectangle _rectI;

		public GDICanvas()
			: base(CreateNewState, CreateStateCopy)
		{
		}

		private static GDICanvasState CreateNewState(object context)
		{
			var canvas = (GDICanvas) context;
			return new GDICanvasState(canvas.Graphics);
		}

		private static GDICanvasState CreateStateCopy(GDICanvasState prototype)
		{
			return new GDICanvasState(prototype);
		}

		public System.Drawing.Graphics Graphics
		{
			get => _graphics;
			set
			{
				if (_graphics != value)
				{
					_graphics = value;
					_graphics.SmoothingMode = SmoothingMode.AntiAlias;
					_graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

					ResetState();
				}
				else
				{
					ResetState();
				}
			}
		}

		public override float MiterLimit
		{
			set => CurrentState.StrokeMiterLimit = value;
		}

		public override Color StrokeColor
		{
			set => CurrentState.StrokeColor = value?.AsColor() ?? System.Drawing.Color.Black;
		}

		public override LineCap StrokeLineCap
		{
			set
			{
				switch (value)
				{
					case LineCap.Round:
						CurrentState.StrokeLineCap = Drawing.Drawing2D.LineCap.Round;
						break;
					case LineCap.Square:
						CurrentState.StrokeLineCap = Drawing.Drawing2D.LineCap.Square;
						break;
					default:
						CurrentState.StrokeLineCap = Drawing.Drawing2D.LineCap.Flat;
						break;
				}
			}
		}

		public override LineJoin StrokeLineJoin
		{
			set
			{
				switch (value)
				{
					case LineJoin.Bevel:
						CurrentState.StrokeLineJoin = System.Drawing.Drawing2D.LineJoin.Bevel;
						break;
					case LineJoin.Round:
						CurrentState.StrokeLineJoin = Drawing.Drawing2D.LineJoin.Round;
						break;
					default:
						CurrentState.StrokeLineJoin = Drawing.Drawing2D.LineJoin.Miter;
						break;
				}
			}
		}

		public override Color FillColor
		{
			set => CurrentState.FillColor = value?.AsColor() ?? Drawing.Color.White;
		}

		public override Color FontColor
		{
			set => CurrentState.TextColor = value?.AsColor() ?? Drawing.Color.Black;
		}

		public override IFont Font
		{
			set
			{
				CurrentState.Font = value;
			}
		}

		public override float FontSize
		{
			set => CurrentState.FontSize = value;
		}


		public override float Alpha
		{
			set
			{
				if (value < 1)
				{
					System.Diagnostics.Debug.WriteLine("Not implemented");
				}
			}
		}

		public override bool Antialias
		{
			set => System.Diagnostics.Debug.WriteLine("Not implemented");
		}

		public override BlendMode BlendMode
		{
			set => System.Diagnostics.Debug.WriteLine("Not implemented");
		}

		public override void SubtractFromClip(float x, float y, float width, float height)
		{
			var region = new Region(new Drawing.RectangleF(x, y, width, height));
			_graphics.ExcludeClip(region);
			region.Dispose();
		}

		protected override void PlatformDrawLine(float x1, float y1, float x2, float y2)
		{
			Draw(g => g.DrawLine(CurrentState.StrokePen, x1, y1, x2, y2));
		}

		protected override void PlatformDrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
		{
			while (startAngle < 0)
			{
				startAngle += 360;
			}

			while (endAngle < 0)
			{
				endAngle += 360;
			}

			float sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);
			SetRect(x, y, width, height);
			if (!clockwise)
			{
				startAngle = endAngle;
			}

			startAngle *= -1;
			if (closed)
			{
				var path = CreatePathForArc(_rect, startAngle, sweep, true);
				Draw(g => g.DrawPath(CurrentState.StrokePen, path));
			}
			else
			{
				Draw(g => g.DrawArc(CurrentState.StrokePen, _rect, startAngle, sweep));
			}
		}

		private GraphicsPath CreatePathForArc(Drawing.RectangleF arcRect, float startAngle, float sweep, bool closed = false)
		{
			var path = new GraphicsPath();
			path.AddArc(arcRect, startAngle, sweep);
			if (closed)
			{
				path.CloseFigure();
			}

			return path;
		}

		public override void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise)
		{
			while (startAngle < 0)
			{
				startAngle += 360;
			}

			while (endAngle < 0)
			{
				endAngle += 360;
			}

			float sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);
			SetRect(x, y, width, height);
			if (!clockwise)
			{
				startAngle = endAngle;
			}

			startAngle *= -1;
			var path = CreatePathForArc(_rect, startAngle, sweep, true);
			Draw(g => g.FillPath(CurrentState.FillBrush, path));
		}

		protected override void PlatformDrawRectangle(float x, float y, float width, float height)
		{
			SetRect(x, y, width, height);
			Draw(g => g.DrawRectangle(CurrentState.StrokePen, _rect.X, _rect.Y, _rect.Width, _rect.Height));
		}

		public override void FillRectangle(float x, float y, float width, float height)
		{
			SetRect(x, y, width, height);
			Draw(g => g.FillRectangle(CurrentState.FillBrush, _rect.X, _rect.Y, _rect.Width, _rect.Height));
		}

		protected override void PlatformDrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			if (cornerRadius == 0)
			{
				PlatformDrawRectangle(x, y, width, height);
				return;
			}

			SetRect(x, y, width, height);

			float minEdgeLength = Math.Min(_rect.Width, _rect.Height);
			cornerRadius = Math.Min(cornerRadius, minEdgeLength / 2);
			float cornerDiameter = cornerRadius * 2;

			var path = new GraphicsPath();
			path.AddArc(_rect.X, _rect.Y, cornerDiameter, cornerDiameter, 180, 90);
			path.AddArc(_rect.X + _rect.Width - cornerDiameter, _rect.Y, cornerDiameter, cornerDiameter, 270, 90);
			path.AddArc(_rect.X + _rect.Width - cornerDiameter, _rect.Y + _rect.Height - cornerDiameter, cornerDiameter, cornerDiameter, 0, 90);
			path.AddArc(_rect.X, _rect.Y + _rect.Height - cornerDiameter, cornerDiameter, cornerDiameter, 90, 90);
			path.CloseAllFigures();

			// ReSharper disable once AccessToDisposedClosure
			Draw(g => g.DrawPath(CurrentState.StrokePen, path));

			path.Dispose();
		}

		private GraphicsPath GetPath(PathF path)
		{
			var graphicsPath = path.PlatformPath as GraphicsPath;

			if (graphicsPath == null)
			{
				graphicsPath = path.AsGDIPath();
				path.PlatformPath = graphicsPath;
			}

			return graphicsPath;
		}

		protected override void PlatformDrawPath(PathF path)
		{
			if (path == null)
			{
				return;
			}

			var vGeometry = GetPath(path);

			Draw(g =>
			{
				g.DrawPath(CurrentState.StrokePen, vGeometry);
			});
		}

		public override void FillPath(PathF path, WindingMode windingMode)
		{
			if (path == null)
			{
				return;
			}

			var graphicsPath = GetPath(path);
			graphicsPath.FillMode = windingMode == WindingMode.NonZero ? FillMode.Winding : FillMode.Alternate;
			Draw(g => g.FillPath(CurrentState.FillBrush, graphicsPath));
		}

		public override void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
		{
			if (path == null)
			{
				return;
			}

			var graphicsPath = GetPath(path);
			graphicsPath.FillMode = windingMode == WindingMode.NonZero ? FillMode.Winding : FillMode.Alternate;
			var region = new Region(graphicsPath);
			_graphics.IntersectClip(region);
		}

		public override void ClipRectangle(float x, float y, float width, float height)
		{
			var region = new Region(new Drawing.RectangleF(x, y, width, height));
			_graphics.IntersectClip(region);
		}

		public override void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			if (cornerRadius == 0)
			{
				FillRectangle(x, y, width, height);
				return;
			}

			SetRect(x, y, width, height);

			float minEdgeLength = Math.Min(_rect.Width, _rect.Height);
			cornerRadius = Math.Min(cornerRadius, minEdgeLength / 2);
			float cornerDiameter = cornerRadius * 2;

			var path = new GraphicsPath();
			path.AddArc(_rect.X, _rect.Y, cornerDiameter, cornerDiameter, 180, 90);
			path.AddArc(_rect.X + _rect.Width - cornerDiameter, _rect.Y, cornerDiameter, cornerDiameter, 270, 90);
			path.AddArc(_rect.X + _rect.Width - cornerDiameter, _rect.Y + _rect.Height - cornerDiameter, cornerDiameter, cornerDiameter, 0, 90);
			path.AddArc(_rect.X, _rect.Y + _rect.Height - cornerDiameter, cornerDiameter, cornerDiameter, 90, 90);
			path.CloseAllFigures();

			Draw(g => g.FillPath(CurrentState.FillBrush, path));

			path.Dispose();
		}

		protected override void PlatformDrawEllipse(float x, float y, float width, float height)
		{
			SetRect(x, y, width, height);
			Draw(g => g.DrawEllipse(CurrentState.StrokePen, _rect));
		}

		public override void FillEllipse(float x, float y, float width, float height)
		{
			SetRect(x, y, width, height);
			Draw(g => g.FillEllipse(CurrentState.FillBrush, _rect));
		}

		public override void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment)
		{
			var font = CurrentState.Font;
			var size = _graphics.MeasureString(value, font.ToSystemDrawingFont(CurrentState.NativeFontSize));

			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Right:
					x -= size.Width;
					break;
				case HorizontalAlignment.Center:
				case HorizontalAlignment.Justified:
					x -= size.Width / 2;
					break;
			}

			Draw(g => g.DrawString(value, font.ToSystemDrawingFont(CurrentState.NativeFontSize), CurrentState.TextBrush, x, y - size.Height));
		}

		public override void DrawString(string value, float x, float y, float width, float height, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment,
			TextFlow textFlow = TextFlow.ClipBounds, float lineAdjustment = 0)
		{
			var font = CurrentState.Font;
			var format = new StringFormat();

			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Left:
					format.Alignment = StringAlignment.Near;
					break;
				case HorizontalAlignment.Center:
					format.Alignment = StringAlignment.Center;
					break;
				case HorizontalAlignment.Right:
					format.Alignment = StringAlignment.Far;
					break;
				default:
					format.Alignment = StringAlignment.Near;
					break;
			}

			switch (verticalAlignment)
			{
				case VerticalAlignment.Top:
					format.LineAlignment = StringAlignment.Near;
					break;
				case VerticalAlignment.Center:
					format.LineAlignment = StringAlignment.Center;
					break;
				case VerticalAlignment.Bottom:
					format.LineAlignment = StringAlignment.Far;
					break;
			}

			SetRect(x, y, width, height);

			if (textFlow == TextFlow.OverflowBounds)
			{
				var size = _graphics.MeasureString(value, font.ToSystemDrawingFont(CurrentState.NativeFontSize), (int) width, format);

				if (size.Height > _rect.Height)
				{
					var difference = size.Height - _rect.Height;

					switch (verticalAlignment)
					{
						case VerticalAlignment.Center:
							_rect.Y -= difference / 2;
							break;
						case VerticalAlignment.Bottom:
							_rect.Y -= difference;
							break;
					}

					_rect.Height = size.Height;
				}
			}

			Draw(g => g.DrawString(value, font.ToSystemDrawingFont(CurrentState.NativeFontSize), CurrentState.TextBrush, _rect, format));
		}

		public override void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
		}

		protected override void PlatformRotate(float degrees, float radians, float x, float y)
		{
			CurrentState.NativeRotate(degrees, x, y);
		}

		protected override void PlatformRotate(float degrees, float radians)
		{
			CurrentState.NativeRotate(degrees);
		}

		protected override void PlatformScale(float sx, float sy)
		{
			CurrentState.NativeScale(sx, sy);
		}

		protected override void PlatformTranslate(float tx, float ty)
		{
			CurrentState.NativeTranslate(tx, ty);
		}

		protected override void PlatformConcatenateTransform(Matrix3x2 transform)
		{
			CurrentState.NativeConcatenateTransform(transform);
		}

		public override void SetShadow(SizeF offset, float blur, Color color)
		{
			System.Diagnostics.Debug.WriteLine("Not implemented");
		}

		protected override float PlatformStrokeSize
		{
			set => CurrentState.StrokeWidth = value;
		}

		protected override void PlatformSetStrokeDashPattern(float[] pattern, float strokeSize)
		{
		}

		public override void SetFillPaint(Paint paint, RectangleF rectangle)
		{
			if (paint == null)
			{
				CurrentState.FillColor = Drawing.Color.White;
				return;
			}

			if (paint is SolidPaint solidPaint)
			{
				CurrentState.FillColor = solidPaint.Color.AsColor();
				return;
			}

			if (paint is PatternPaint)
			{
				CurrentState.StrokeColor = paint.ForegroundColor.AsColor();
				CurrentState.FillColor = paint.BackgroundColor.AsColor();
				return;
			}

			if (paint is LinearGradientPaint linearGradientPaint)
			{
				float x1 = (float)(linearGradientPaint.StartPoint.X * rectangle.Width) + rectangle.X;
				float y1 = (float)(linearGradientPaint.StartPoint.Y * rectangle.Height) + rectangle.Y;

				float x2 = (float)(linearGradientPaint.EndPoint.X * rectangle.Width) + rectangle.X;
				float y2 = (float)(linearGradientPaint.EndPoint.Y * rectangle.Height) + rectangle.Y;

				Drawing.PointF point1 = new Drawing.PointF(x1, y1);
				Drawing.PointF point2 = new Drawing.PointF(x2, y2);
				Drawing.Color color1 = linearGradientPaint.StartColor.AsColor();
				Drawing.Color color2 = linearGradientPaint.EndColor.AsColor();

				CurrentState.SetFillLinear(point1, point2, color1, color2);

				return;
			}

			if (paint is RadialGradientPaint radialGradientPaint)
			{
				float x1 = (float)((radialGradientPaint.Center.X - radialGradientPaint.Radius) * rectangle.Width) + rectangle.X;
				float y1 = (float)((radialGradientPaint.Center.Y - radialGradientPaint.Radius) * rectangle.Height) + rectangle.Y;
				float w = rectangle.Width * (float)radialGradientPaint.Radius * 2;
				float h = rectangle.Height * (float)radialGradientPaint.Radius * 2;

				GraphicsPath path = new GraphicsPath();
				path.AddEllipse(x1, y1, w, h);
				Drawing.Color color1 = radialGradientPaint.StartColor.AsColor();
				Drawing.Color color2 = radialGradientPaint.EndColor.AsColor();

				CurrentState.SetFillRadial(path, color1, color2);
			}
		}

		public override void DrawImage(IImage image, float x, float y, float width, float height)
		{
			if (image is GDIImage nativeImage)
			{
				_rectI.X = (int) x;
				_rectI.Y = (int) y;
				_rectI.Width = (int) width;
				_rectI.Height = (int) height;

				Draw(g => g.DrawImage(nativeImage.NativeImage, _rectI, 0f, 0f, image.Width, image.Height, GraphicsUnit.Pixel));
			}
		}

		private void SetRect(float x, float y, float width, float height)
		{
			_rect.X = x;
			_rect.Y = y;
			_rect.Width = width;
			_rect.Height = height;
		}

		private void Draw(Action<System.Drawing.Graphics> drawingAction)
		{
			if (CurrentState.IsShadowed)
			{
				DrawShadow(drawingAction);
			}

			if (CurrentState.IsBlurred)
			{
				DrawBlurred(drawingAction);
			}
			else
			{
				drawingAction(_graphics);
			}
		}

		private void DrawShadow(Action<System.Drawing.Graphics> drawingAction)
		{
		}

		private void DrawBlurred(Action<System.Drawing.Graphics> drawingAction)
		{
		}

		public override SizeF GetStringSize(string aString, IFont font, float fontSize)
		{
			var f = font.ToSystemDrawingFont(fontSize * GDICanvasState.DpiAdjustment);
			var size = TextRenderer.MeasureText(aString, f);
			f.Dispose();
			return new SizeF(size.Width, size.Height);
		}

		public override SizeF GetStringSize(string aString, IFont font, float fontSize, HorizontalAlignment aHorizontalAlignment, VerticalAlignment aVerticalAlignment)
		{
			var f = font.ToSystemDrawingFont(fontSize * GDICanvasState.DpiAdjustment);
			var size = TextRenderer.MeasureText(aString, f);
			f.Dispose();
			return new SizeF(size.Width, size.Height);
		}
	}
}
