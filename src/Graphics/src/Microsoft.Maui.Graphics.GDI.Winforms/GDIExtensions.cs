using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Drawing = System.Drawing;

namespace Microsoft.Maui.Graphics.GDI
{
    public static class GDIGraphicsExtensions
    {
        public static Drawing.RectangleF AsRectangleF(this RectangleF target)
        {
            return new Drawing.RectangleF(target.Left, target.Top, Math.Abs(target.Width), Math.Abs(target.Height));
        }

        public static RectangleF AsRectangleF(this Drawing.RectangleF target)
        {
            return new RectangleF(target.Left, target.Top, Math.Abs(target.Width), Math.Abs(target.Height));
        }

        public static RectangleF AsRectangleF(this global::System.Drawing.Rectangle target)
        {
            return new RectangleF(target.Left, target.Top, Math.Abs(target.Width), Math.Abs(target.Height));
        }

        public static Drawing.SizeF AsSizeF(this SizeF target)
        {
            return new Drawing.SizeF(target.Width, target.Height);
        }

        public static SizeF AsSizeF(this Drawing.SizeF target)
        {
            return new SizeF(target.Width, target.Height);
        }

        public static Drawing.PointF ToPointF(this PointF target)
        {
            return new Drawing.PointF(target.X, target.Y);
        }

        public static Drawing.Color AsColor(this Color color)
        {
            if (color == null) return Drawing.Color.Black;

            var alpha = (int) (color.Alpha * 255);
            var red = (int) (color.Red * 255);
            var green = (int) (color.Green * 255);
            var blue = (int) (color.Blue * 255);

            return Drawing.Color.FromArgb(alpha, red, green, blue);
        }


        public static GraphicsPath AsGDIPath(this PathF target)
        {
            return AsGDIPath(target, 1);
        }

        public static GraphicsPath AsGDIPath(this PathF target, float ppu)
        {
            return AsGDIPath(target, ppu, 0, 0, 1, 1);
        }

        public static GraphicsPath AsGDIPath(this PathF target, float ppu, float ox, float oy, float fx, float fy)
        {
            var path = new GraphicsPath();

#if DEBUG
            try
            {
#endif

                float ppux = ppu * fx;
                float ppuy = ppu * fy;

                int pointIndex = 0;
                var arcAngleIndex = 0;
                var arcClockwiseIndex = 0;

                foreach (var type in target.SegmentTypes)
                {
                    if (type == PathOperation.Move)
                    {
                        path.StartFigure();
                        pointIndex++;
                    }
                    else if (type == PathOperation.Line)
                    {
                        var startPoint = target[pointIndex - 1];
                        var endPoint = target[pointIndex++];
                        path.AddLine(
                            ox + startPoint.X * ppux,
                            oy + startPoint.Y * ppuy,
                            ox + endPoint.X * ppux,
                            oy + endPoint.Y * ppuy);
                    }
                    else if (type == PathOperation.Quad)
                    {
                        var startPoint = target[pointIndex - 1];
                        var quadControlPoint = target[pointIndex++];
                        var endPoint = target[pointIndex++];

                        var cubicControlPoint1X = startPoint.X + 2.0f * (quadControlPoint.X - startPoint.X) / 3.0f;
                        var cubicControlPoint1Y = startPoint.Y + 2.0f * (quadControlPoint.Y - startPoint.Y) / 3.0f;

                        var cubicControlPoint2X = endPoint.X + 2.0f * (quadControlPoint.X - endPoint.X) / 3.0f;
                        var cubicControlPoint2Y = endPoint.Y + 2.0f * (quadControlPoint.Y - endPoint.Y) / 3.0f;

                        path.AddBezier(
                            ox + startPoint.X * ppux,
                            oy + startPoint.Y * ppuy,
                            ox + cubicControlPoint1X * ppux,
                            oy + cubicControlPoint1Y * ppuy,
                            ox + cubicControlPoint2X * ppux,
                            oy + cubicControlPoint2Y * ppuy,
                            ox + endPoint.X * ppux,
                            oy + endPoint.Y * ppuy);
                    }
                    else if (type == PathOperation.Cubic)
                    {
                        var startPoint = target[pointIndex - 1];
                        var cubicControlPoint1 = target[pointIndex++];
                        var cubicControlPoint2 = target[pointIndex++];
                        var endPoint = target[pointIndex++];

                        path.AddBezier(
                            ox + startPoint.X * ppux,
                            oy + startPoint.Y * ppuy,
                            ox + cubicControlPoint1.X * ppux,
                            oy + cubicControlPoint1.Y * ppuy,
                            ox + cubicControlPoint2.X * ppux,
                            oy + cubicControlPoint2.Y * ppuy,
                            ox + endPoint.X * ppux,
                            oy + endPoint.Y * ppuy);
                    }
                    else if (type == PathOperation.Arc)
                    {
                        var topLeft = target[pointIndex++];
                        var bottomRight = target[pointIndex++];
                        var startAngle = target.GetArcAngle(arcAngleIndex++);
                        var endAngle = target.GetArcAngle(arcAngleIndex++);
                        var clockwise = target.GetArcClockwise(arcClockwiseIndex++);

                        while (startAngle < 0)
                        {
                            startAngle += 360;
                        }

                        while (endAngle < 0)
                        {
                            endAngle += 360;
                        }

                        var x = ox + topLeft.X * ppux;
                        var y = oy + topLeft.Y * ppuy;
                        var width = (bottomRight.X - topLeft.X) * ppux;
                        var height = (bottomRight.Y - topLeft.Y) * ppuy;

                        float sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);
                        if (!clockwise)
                        {
                            startAngle = endAngle;
                        }

                        startAngle *= -1;

                        path.AddArc(x, y, width, height, startAngle, sweep);
                    }
                    else if (type == PathOperation.Close)
                    {
                        path.CloseFigure();
                    }
                }

                return path;
#if DEBUG
            }
            catch (Exception exc)
            {
                path.Dispose();

                var definition = target.ToDefinitionString();
                Logger.Debug(string.Format("Unable to convert the path to a GDIPath: {0}", definition), exc);
                return null;
            }
#endif
        }
    }
}
