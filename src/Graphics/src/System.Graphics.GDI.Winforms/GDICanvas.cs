using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Graphics.Text;

namespace System.Graphics.GDI
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
            set => CurrentState.StrokeColor = value?.AsColor() ?? Drawing.Color.Black;
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
                        CurrentState.StrokeLineJoin = Drawing.Drawing2D.LineJoin.Bevel;
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

        public override string FontName
        {
            set
            {
                FontMapping vMapping = GDIFontManager.GetMapping(value);
                CurrentState.FontName = vMapping.Name;
                CurrentState.FontStyle = vMapping.Style;
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
                    Logger.Debug("Not implemented");
                }
            }
        }

        public override bool Antialias
        {
            set => Logger.Debug("Not implemented");
        }

        public override BlendMode BlendMode
        {
            set => Logger.Debug("Not implemented");
        }

        public override void SubtractFromClip(float x, float y, float width, float height)
        {
            var region = new Region(new Drawing.RectangleF(x, y, width, height));
            _graphics.ExcludeClip(region);
            region.Dispose();
        }

        protected override void NativeDrawLine(float x1, float y1, float x2, float y2)
        {
            Draw(g => g.DrawLine(CurrentState.StrokePen, x1, y1, x2, y2));
        }

        protected override void NativeDrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
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

        protected override void NativeDrawRectangle(float x, float y, float width, float height)
        {
            SetRect(x, y, width, height);
            Draw(g => g.DrawRectangle(CurrentState.StrokePen, _rect.X, _rect.Y, _rect.Width, _rect.Height));
        }

        public override void FillRectangle(float x, float y, float width, float height)
        {
            SetRect(x, y, width, height);
            Draw(g => g.FillRectangle(CurrentState.FillBrush, _rect.X, _rect.Y, _rect.Width, _rect.Height));
        }

        protected override void NativeDrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
        {
            var strokeWidth = CurrentState.StrokeWidth;

            SetRect(x, y, width, height);

            if (cornerRadius > _rect.Width / 2)
            {
                cornerRadius = _rect.Width / 2;
            }

            if (cornerRadius > _rect.Height / 2)
            {
                cornerRadius = _rect.Height / 2;
            }

            var path = new GraphicsPath();
            path.AddArc(_rect.X, _rect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(_rect.X + _rect.Width - cornerRadius, _rect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(_rect.X + _rect.Width - cornerRadius, _rect.Y + _rect.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(_rect.X, _rect.Y + _rect.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseAllFigures();

            // ReSharper disable once AccessToDisposedClosure
            Draw(g => g.DrawPath(CurrentState.StrokePen, path));

            path.Dispose();
        }

        private GraphicsPath GetPath(PathF path)
        {
            var graphicsPath = path.NativePath as GraphicsPath;

            if (graphicsPath == null)
            {
                graphicsPath = path.AsGDIPath();
                path.NativePath = graphicsPath;
            }

            return graphicsPath;
        }

        protected override void NativeDrawPath(PathF path)
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
            SetRect(x, y, width, height);

            var path = new GraphicsPath();
            path.AddArc(_rect.X, _rect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(_rect.X + _rect.Width - cornerRadius, _rect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(_rect.X + _rect.Width - cornerRadius, _rect.Y + _rect.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(_rect.X, _rect.Y + _rect.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseAllFigures();

            Draw(g => g.FillPath(CurrentState.FillBrush, path));

            path.Dispose();
        }

        protected override void NativeDrawEllipse(float x, float y, float width, float height)
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
            var size = _graphics.MeasureString(value, font);

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

            Draw(g => g.DrawString(value, font, CurrentState.TextBrush, x, y - size.Height));
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
                var size = _graphics.MeasureString(value, font, (int) width, format);

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

            Draw(g => g.DrawString(value, font, CurrentState.TextBrush, _rect, format));
        }

        public override void DrawText(IAttributedText value, float x, float y, float width, float height)
        {
        }

        protected override void NativeRotate(float degrees, float radians, float x, float y)
        {
            CurrentState.NativeRotate(degrees, x, y);
        }

        protected override void NativeRotate(float degrees, float radians)
        {
            CurrentState.NativeRotate(degrees);
        }

        protected override void NativeScale(float sx, float sy)
        {
            CurrentState.NativeScale(sx, sy);
        }

        protected override void NativeTranslate(float tx, float ty)
        {
            CurrentState.NativeTranslate(tx, ty);
        }

        protected override void NativeConcatenateTransform(AffineTransform transform)
        {
            CurrentState.NativeConcatenateTransform(transform);
        }

        public override void SetShadow(SizeF offset, float blur, Color color)
        {
            Logger.Debug("Not implemented");
        }

        protected override float NativeStrokeSize
        {
            set => CurrentState.StrokeWidth = value;
        }

        protected override void NativeSetStrokeDashPattern(float[] pattern, float strokeSize)
        {
        }

        public override void SetFillPaint(Paint paint, float x1, float y1, float x2, float y2)
        {
            if (paint == null)
            {
                CurrentState.FillColor = Drawing.Color.White;
                return;
            }

            if (paint.PaintType == PaintType.Solid)
            {
                CurrentState.FillColor = paint.StartColor.AsColor();
                return;
            }

            if (paint.PaintType == PaintType.Pattern)
            {
                CurrentState.StrokeColor = paint.ForegroundColor.AsColor();
                CurrentState.FillColor = paint.BackgroundColor.AsColor();
                return;
            }

/*            if (paint.PaintType == PaintType.LinearGradient)
            {
                point1.X = x1;
                point1.Y = y1;
                point2.X = x2;
                point2.Y = y2;
                currentState.SetLinearGradient(paint, point1, point2);
            }
            else
            {
                point1.X = x1;
                point1.Y = y1;
                point2.X = x2;
                point2.Y = y2;
                currentState.SetRadialGradient(paint, point1, point2);
            }*/
        }

        public override void SetToSystemFont()
        {
            CurrentState.FontName = "Arial";
        }

        public override void SetToBoldSystemFont()
        {
            CurrentState.FontName = "Arial";
            CurrentState.FontStyle = FontStyle.Bold;
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
    }
}