using System;
using System.Collections.Generic;
using CoreGraphics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
    public class PathData
    {
        public CGPath? Data { get; set; }
        public bool IsNonzeroFillRule { get; set; }
    }

    public static class GeometryExtensions
    {
        public static PathData ToNative(this IGeometry geometry)
        {
            PathData pathData = new PathData
            {
                Data = new CGPath()
            };

            CGAffineTransform transform = CGAffineTransform.MakeIdentity();

            if (geometry is LineGeometry lineGeometry)
            {
                pathData.Data.MoveToPoint(transform, lineGeometry.StartPoint.ToNative());
                pathData.Data.AddLineToPoint(transform, lineGeometry.EndPoint.ToNative());
            }
            else if (geometry is RectangleGeometry rectangleGeometry)
            {
                Rect rect = rectangleGeometry.Rect;
                pathData.Data.AddRect(transform, new CGRect(rect.X, rect.Y, rect.Width, rect.Height));
            }
            else if (geometry is EllipseGeometry ellipseGeometry)
            {
                CGRect rect = new CGRect(
                    ellipseGeometry.Center.X - ellipseGeometry.RadiusX,
                    ellipseGeometry.Center.Y - ellipseGeometry.RadiusY,
                    ellipseGeometry.RadiusX * 2,
                    ellipseGeometry.RadiusY * 2);

                pathData.Data.AddEllipseInRect(transform, rect);
            }
            else if (geometry is GeometryGroup geometryGroup)
            {
                pathData.IsNonzeroFillRule = geometryGroup.FillRule == FillRule.Nonzero;

                foreach (Geometry child in geometryGroup.Children)
                {
                    PathData pathChild = child.ToNative();
                    pathData.Data.AddPath(pathChild.Data);
                }
            }
            else if (geometry is PathGeometry pathGeometry)
            {
                pathData.IsNonzeroFillRule = pathGeometry.FillRule == FillRule.Nonzero;

                foreach (PathFigure pathFigure in pathGeometry.Figures)
                {
                    pathData.Data.MoveToPoint(transform, pathFigure.StartPoint.ToNative());
                    Point lastPoint = pathFigure.StartPoint;

                    foreach (PathSegment pathSegment in pathFigure.Segments)
                    {
                        // LineSegment
                        if (pathSegment is LineSegment lineSegment)
                        {
                            pathData.Data.AddLineToPoint(transform, lineSegment.Point.ToNative());
                            lastPoint = lineSegment.Point;
                        }
                        // PolyLineSegment
                        else if (pathSegment is PolyLineSegment polylineSegment)
                        {
                            PointCollection points = polylineSegment.Points;

                            for (int i = 0; i < points.Count; i++)
                                pathData.Data.AddLineToPoint(transform, points[i].ToNative());

                            lastPoint = points[^1];
                        }

                        // BezierSegment
                        if (pathSegment is BezierSegment bezierSegment)
                        {
                            pathData.Data.AddCurveToPoint(
                                transform,
                                bezierSegment.Point1.ToNative(),
                                bezierSegment.Point2.ToNative(),
                                bezierSegment.Point3.ToNative());

                            lastPoint = bezierSegment.Point3;
                        }
                        // PolyBezierSegment
                        else if (pathSegment is PolyBezierSegment polyBezierSegment)
                        {
                            PointCollection points = polyBezierSegment.Points;

                            if (points.Count >= 3)
                            {
                                for (int i = 0; i < points.Count; i += 3)
                                {
                                    pathData.Data.AddCurveToPoint(
                                        transform,
                                        points[i].ToNative(),
                                        points[i + 1].ToNative(),
                                        points[i + 2].ToNative());
                                }
                            }

                            lastPoint = points[^1];
                        }

                        // QuadraticBezierSegment
                        if (pathSegment is QuadraticBezierSegment quadraticBezierSegment)
                        {
                            pathData.Data.AddQuadCurveToPoint(
                                transform,
                                new nfloat(quadraticBezierSegment.Point1.X),
                                new nfloat(quadraticBezierSegment.Point1.Y),
                                new nfloat(quadraticBezierSegment.Point2.X),
                                new nfloat(quadraticBezierSegment.Point2.Y));

                            lastPoint = quadraticBezierSegment.Point2;
                        }
                        // PolyQuadraticBezierSegment
                        else if (pathSegment is PolyQuadraticBezierSegment polyBezierSegment)
                        {
                            PointCollection points = polyBezierSegment.Points;

                            if (points.Count >= 2)
                            {
                                for (int i = 0; i < points.Count; i += 2)
                                {
                                    pathData.Data.AddQuadCurveToPoint(
                                        transform,
                                        new nfloat(points[i + 0].X),
                                        new nfloat(points[i + 0].Y),
                                        new nfloat(points[i + 1].X),
                                        new nfloat(points[i + 1].Y));
                                }
                            }

                            lastPoint = points[^1];
                        }
                        // ArcSegment
                        else if (pathSegment is ArcSegment arcSegment)
                        {
                            List<Point> points = new List<Point>();

                            GeometryHelper.FlattenArc(
                                points,
                                lastPoint,
                                arcSegment.Point,
                                arcSegment.Size.Width,
                                arcSegment.Size.Height,
                                arcSegment.RotationAngle,
                                arcSegment.IsLargeArc,
                                arcSegment.SweepDirection == SweepDirection.CounterClockwise,
                                1);

                            CGPoint[] cgpoints = new CGPoint[points.Count];

                            for (int i = 0; i < points.Count; i++)
                                cgpoints[i] = transform.TransformPoint(points[i].ToNative());

                            pathData.Data.AddLines(cgpoints);

                            lastPoint = points.Count > 0 ? points[^1] : Point.Zero;
                        }
                    }

                    if (pathFigure.IsClosed)
                        pathData.Data.CloseSubpath();
                }
            }

            return pathData;
        }
    }
}