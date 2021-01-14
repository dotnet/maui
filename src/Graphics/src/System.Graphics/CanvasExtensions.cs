namespace System.Graphics
{
    public static class CanvasExtensions
    {
        public static void DrawLine(this ICanvas target, PointF point1, PointF point2)
        {
            target.DrawLine(point1.X, point1.Y, point2.X, point2.Y);
        }

        public static void DrawRectangle(this ICanvas target, RectangleF rect)
        {
            target.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void FillRectangle(this ICanvas target, RectangleF rect)
        {
            target.FillRectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void DrawRoundedRectangle(this ICanvas target, RectangleF rect, float cornerRadius)
        {
            target.DrawRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius);
        }

        public static void FillRoundedRectangle(this ICanvas target, RectangleF rect, float cornerRadius)
        {
            target.FillRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius);
        }

        public static void DrawEllipse(this ICanvas target, RectangleF rect)
        {
            target.DrawEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void FillEllipse(this ICanvas target, RectangleF rect)
        {
            target.FillEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        [Obsolete]
        public static void DrawOval(this ICanvas target, RectangleF rect)
        {
            target.DrawEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        [Obsolete]
        public static void DrawOval(this ICanvas target, float x, float y, float width, float height)
        {
            target.DrawEllipse(x,y,width, height);
        }

        [Obsolete]
        public static void FillOval(this ICanvas target, RectangleF rect)
        {
            target.FillEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        [Obsolete]
        public static void FillOval(this ICanvas target, float x, float y, float width, float height)
        {
            target.FillEllipse(x, y, width, height);
        }

        public static void DrawPath(this ICanvas target, PathF path)
        {
            target.DrawPath(path);
        }

        public static void FillPath(this ICanvas target, PathF path)
        {
            target.FillPath(path, WindingMode.NonZero);
        }

        public static void FillPath(this ICanvas target, PathF path, WindingMode windingMode)
        {
            target.FillPath(path, windingMode);
        }

        public static void ClipPath(this ICanvas target, PathF path, WindingMode windingMode = WindingMode.NonZero)
        {
            target.ClipPath(path, windingMode);
        }

        public static void ClipRectangle(this ICanvas target, RectangleF rect)
        {
            target.ClipRectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void DrawString(
            this ICanvas target, 
            string value, 
            RectangleF bounds, 
            HorizontalAlignment horizontalAlignment, 
            VerticalAlignment verticalAlignment,
            TextFlow textFlow = TextFlow.ClipBounds, 
            float lineSpacingAdjustment = 0)
        {
            target.DrawString(value, bounds.X, bounds.Y, bounds.Width, bounds.Height, horizontalAlignment, verticalAlignment, textFlow, lineSpacingAdjustment);
        }

        public static void FillCircle(this ICanvas target, float centerX, float centerY, float radius)
        {
            var x = centerX - radius;
            var y = centerY - radius;
            var size = radius * 2;

            target.FillEllipse(x, y, size, size);
        }

        public static void FillCircle(this ICanvas target, PointF center, float radius)
        {
            var x = center.X - radius;
            var y = center.Y - radius;
            var size = radius * 2;

            target.FillEllipse(x, y, size, size);
        }

        public static void DrawCircle(this ICanvas target, float centerX, float centerY, float radius)
        {
            var x = centerX - radius;
            var y = centerY - radius;
            var size = radius * 2;

            target.DrawEllipse(x, y, size, size);
        }

        public static void DrawCircle(this ICanvas target, PointF center, float radius)
        {
            var x = center.X - radius;
            var y = center.Y - radius;
            var size = radius * 2;

            target.DrawEllipse(x, y, size, size);
        }

        /// <summary>
        /// Fills the arc with the specified paint.  This is a helper method for when filling
        /// an arc with a gradient, so that you don't need to worry about calculating the gradient
        /// handle locations based on the rectangle size and location.
        /// </summary>
        /// <param name="canvas">canvas</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height</param>
        /// <param name="startAngle">The start angle</param>
        /// <param name="endAngle">The end angle</param>
        /// <param name="paint">The paint</param>
        /// <param name="clockwise">The direction to draw the arc</param>
        public static void FillArc(this ICanvas canvas, float x, float y, float width, float height, float startAngle, float endAngle, Paint paint, bool clockwise)
        {
            var rectangle = new RectangleF(x, y, width, height);
            canvas.SetFillPaint(paint, rectangle);
            canvas.FillArc(x, y, width, height, startAngle, endAngle, clockwise);
        }

        /// <summary>
        /// Draws the arc.  This is a helper method to draw an arc when you have a rectangle already defined
        /// for the ellipse bounds.
        /// </summary>
        /// <param name="canvas">canvas</param>
        /// <param name="bounds">The ellipse bounds.</param>
        /// <param name="startAngle">The start angle</param>
        /// <param name="endAngle">The end angle</param>
        /// <param name="clockwise">The direction to draw the arc</param>
        /// <param name="closed">If the arc is closed or not</param>
        public static void DrawArc(this ICanvas canvas, RectangleF bounds, float startAngle, float endAngle, bool clockwise, bool closed)
        {
            canvas.DrawArc(bounds.X, bounds.Y, bounds.Width, bounds.Height, startAngle, endAngle, clockwise, closed);
        }

        /// <summary>
        /// Fills the arc.  This is a helper method to fill an arc when you have a rectangle already defined
        /// for the ellipse bounds.
        /// </summary>
        /// <param name="canvas">canvas</param>
        /// <param name="bounds">The ellipse bounds.</param>
        /// <param name="startAngle">The start angle</param>
        /// <param name="endAngle">The end angle</param>
        /// <param name="clockwise">The direction to draw the arc</param>
        public static void FillArc(this ICanvas canvas, RectangleF bounds, float startAngle, float endAngle, bool clockwise)
        {
            canvas.FillArc(bounds.X, bounds.Y, bounds.Width, bounds.Height, startAngle, endAngle, clockwise);
        }

        /// <summary>
        /// Enables the default shadow.
        /// </summary>
        /// <param name="canvas">canvas</param>
        /// <param name="zoom">Zoom.</param>
        public static void EnableDefaultShadow(this ICanvas canvas, float zoom = 1)
        {
            canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
        }

        /// <summary>
        /// Resets the stroke to the default settings:
        ///  - Stroke Size: 1
        ///  - Stroke Dash Pattern: None
        ///  - Stroke Location: Center
        ///  - Stroke Line Join: Miter
        ///  - Stroke Line Cap: Butt
        ///  - Stroke Brush: None
        ///  - Stroke Color: Black
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        public static void ResetStroke(this ICanvas canvas)
        {
            canvas.StrokeSize = 1;
            canvas.StrokeDashPattern = null;
            canvas.StrokeLineJoin = LineJoin.Miter;
            canvas.StrokeLineCap = LineCap.Butt;
            canvas.StrokeColor = Colors.Black;
        }
        
        public static void SetFillPattern(this ICanvas target, IPattern pattern)
        {
            SetFillPattern(target, pattern, Colors.Black);
        }

        public static void SetFillPattern(
            this ICanvas target,
            IPattern pattern,
            Color foregroundColor)
        {
            if (target != null)
            {
                if (pattern != null)
                {
                    var paint = pattern.AsPaint(foregroundColor);
                    target.SetFillPaint(paint, 0, 0, 0, 0);
                }
                else
                {
                    target.FillColor = Colors.White;
                }
            }
        }
        
        public static void SubtractFromClip(this ICanvas target, RectangleF rect)
        {
            target.SubtractFromClip(rect.X, rect.Y, rect.Width, rect.Height);
        }
        
        public static void SetFillPaint(this ICanvas target, Paint paint, PointF point1, PointF point2)
        {
            target.SetFillPaint(paint, point1.X, point1.Y, point2.X, point2.Y);
        }
        
        public static void SetFillPaint(this ICanvas target, Paint paint, RectangleF rectangle)
        {
            target.SetFillPaint(paint, rectangle.Left, rectangle.Top, rectangle.Bottom, rectangle.Right);
        }
    }
}