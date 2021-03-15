using SharpDX;
using SharpDX.Direct2D1;
using System;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public static class DXExtensions
    {
        public static Color4 AsDxColor(this Color target)
        {
            if (target == null)
                return global::SharpDX.Color.Black;

            return new Color4(new float[] {target.Red, target.Green, target.Blue, target.Alpha});
        }

        public static Color4 AsDxColor(this Color target, float aAlphaMultiplier)
        {
            if (target == null)
            {
                return global::SharpDX.Color.Black;
            }

            return new Color4(target.Red, target.Green, target.Blue, target.Alpha * aAlphaMultiplier);
        }

        public static PathGeometry AsDxPath(this PathF target, Factory factory, FillMode fillMode = FillMode.Winding)
        {
            return AsDxPath(target, 1, factory, fillMode);
        }

        public static PathGeometry AsDxPath(this PathF path, float ppu, Factory factory, FillMode fillMode = FillMode.Winding)
        {
            return AsDxPath(path, ppu, 0, 0, 1, 1, factory, fillMode);
        }

        public static PathGeometry AsDxPath(this PathF path, float ppu, float ox, float oy, float fx, float fy, Factory factory, FillMode fillMode = FillMode.Winding)
        {
            var geometry = new PathGeometry(factory);

#if DEBUG
            try
            {
#endif

                var sink = geometry.Open();
                sink.SetFillMode(fillMode);

                var ppux = ppu * fx;
                var ppuy = ppu * fy;

                var pointIndex = 0;
                var arcAngleIndex = 0;
                var arcClockwiseIndex = 0;
                var figureOpen = false;
                var segmentIndex = -1;

                var lastOperation = PathOperation.Move;

                foreach (var type in path.SegmentTypes)
                {
                    segmentIndex++;

                    if (type == PathOperation.Move)
                    {
                        if (lastOperation != PathOperation.Close && lastOperation != PathOperation.Move)
                        {
                            sink.EndFigure(FigureEnd.Open);
                            //vPath = vPathGeometry.Open();
                        }

                        var point = path[pointIndex++];
                        /*var vBegin = FigureBegin.Hollow;
                        if (path.IsSubPathClosed(vSegmentIndex))
                        {
                            vBegin = FigureBegin.Filled;
                        }*/
                        var begin = FigureBegin.Filled;
                        sink.BeginFigure(ox + point.X * ppux, oy + point.Y * ppuy, begin);
                        figureOpen = true;
                    }
                    else if (type == PathOperation.Line)
                    {
                        var point = path[pointIndex++];
                        sink.LineTo(ox + point.X * ppux, oy + point.Y * ppuy);
                    }

                    else if (type == PathOperation.Quad)
                    {
                        var controlPoint = path[pointIndex++];
                        var endPoint = path[pointIndex++];

                        sink.QuadTo(
                            ox + controlPoint.X * ppux,
                            oy + controlPoint.Y * ppuy,
                            ox + endPoint.X * ppux,
                            oy + endPoint.Y * ppuy);
                    }
                    else if (type == PathOperation.Cubic)
                    {
                        var controlPoint1 = path[pointIndex++];
                        var controlPoint2 = path[pointIndex++];
                        var endPoint = path[pointIndex++];
                        sink.CubicTo(
                            ox + controlPoint1.X * ppux,
                            oy + controlPoint1.Y * ppuy,
                            ox + controlPoint2.X * ppux,
                            oy + controlPoint2.Y * ppuy,
                            ox + endPoint.X * ppux,
                            oy + endPoint.Y * ppuy);
                    }
                    else if (type == PathOperation.Arc)
                    {
                        var topLeft = path[pointIndex++];
                        var bottomRight = path[pointIndex++];
                        var startAngle = path.GetArcAngle(arcAngleIndex++);
                        var endAngle = path.GetArcAngle(arcAngleIndex++);
                        var clockwise = path.GetArcClockwise(arcClockwiseIndex++);

                        while (startAngle < 0)
                        {
                            startAngle += 360;
                        }

                        while (endAngle < 0)
                        {
                            endAngle += 360;
                        }

                        var rotation = Geometry.GetSweep(startAngle, endAngle, clockwise);
                        var absRotation = Math.Abs(rotation);

                        var rectX = ox + topLeft.X * ppux;
                        var rectY = oy + topLeft.Y * ppuy;
                        var rectWidth = (ox + bottomRight.X * ppux) - rectX;
                        var rectHeight = (oy + bottomRight.Y * ppuy) - rectY;

                        var startPoint = Geometry.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -startAngle);
                        var endPoint = Geometry.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -endAngle);


                        if (!figureOpen)
                        {
                            /*var vBegin = FigureBegin.Hollow;
                            if (path.IsSubPathClosed(vSegmentIndex))
                            {
                                vBegin = FigureBegin.Filled;
                            }*/
                            var begin = FigureBegin.Filled;
                            sink.BeginFigure(startPoint.X, startPoint.Y, begin);
                            figureOpen = true;
                        }
                        else
                        {
                            sink.LineTo(startPoint.X, startPoint.Y);
                        }

                        var arcSegment = new ArcSegment
                        {
                            Point = new Vector2(endPoint.X, endPoint.Y),
                            Size = new Size2F(rectWidth / 2, rectHeight / 2),
                            SweepDirection = clockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
                            ArcSize = absRotation >= 180 ? ArcSize.Large : ArcSize.Small
                        };
                        sink.AddArc(arcSegment);
                    }
                    else if (type == PathOperation.Close)
                    {
                        sink.EndFigure(FigureEnd.Closed);
                    }

                    lastOperation = type;
                }

                if (segmentIndex >= 0 && lastOperation != PathOperation.Close)
                {
                    sink.EndFigure(FigureEnd.Open);
                }

                sink.Close();

                return geometry;
#if DEBUG
            }
            catch (Exception exc)
            {
                geometry.Dispose();

                var definition = path.ToDefinitionString();
                Logger.Debug($"Unable to convert the path to a DXPath: {definition}", exc);
                return null;
            }
#endif
        }  
    }
}