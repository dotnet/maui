using System.Collections.Generic;
using Microsoft.Maui.Controls.Shapes;
using SkiaSharp;
using Point = Microsoft.Maui.Graphics.Point;
using Rect = Microsoft.Maui.Graphics.Rect;


namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class GeometryExtensions
	{
		public static SKPath ToSKPath(this Geometry geometry)
		{
#pragma warning disable IL2026
			return geometry == null ? MakePath(geometry) : MakePath((dynamic)geometry);
#pragma warning disable IL2026
		}

		static SKPath MakePath(Geometry geometry)
		{
			return new SKPath();
		}

		static SKPath MakePath(LineGeometry lineGeometry)
		{
			var path = new SKPath();
			path.MoveTo(
				Forms.ConvertToScaledPixel(lineGeometry.StartPoint.X),
				Forms.ConvertToScaledPixel(lineGeometry.StartPoint.Y));

			path.LineTo(
				Forms.ConvertToScaledPixel(lineGeometry.EndPoint.X),
				Forms.ConvertToScaledPixel(lineGeometry.EndPoint.Y));

			return path;
		}

		static SKPath MakePath(RectangleGeometry rectangleGeometry)
		{
			var path = new SKPath();
			Rect rect = rectangleGeometry.Rect;

			path.AddRect(new SKRect(
				Forms.ConvertToScaledPixel(rect.Left),
				Forms.ConvertToScaledPixel(rect.Top),
				Forms.ConvertToScaledPixel(rect.Right),
				Forms.ConvertToScaledPixel(rect.Bottom)),
				SKPathDirection.Clockwise);

			return path;
		}

		static SKPath MakePath(EllipseGeometry ellipseGeometry)
		{
			var path = new SKPath();
			path.AddOval(new SKRect(
					Forms.ConvertToScaledPixel(ellipseGeometry.Center.X - ellipseGeometry.RadiusX),
					Forms.ConvertToScaledPixel(ellipseGeometry.Center.Y - ellipseGeometry.RadiusY),
					Forms.ConvertToScaledPixel(ellipseGeometry.Center.X + ellipseGeometry.RadiusX),
					Forms.ConvertToScaledPixel(ellipseGeometry.Center.Y + ellipseGeometry.RadiusY)),
					SKPathDirection.Clockwise);

			return path;
		}

		static SKPath MakePath(GeometryGroup geometryGroup)
		{
			var path = new SKPath();
			path.FillType = geometryGroup.FillRule == FillRule.Nonzero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;

			foreach (Geometry child in geometryGroup.Children)
			{
#pragma warning disable IL2026
				SKPath childPath = MakePath((dynamic)child);
#pragma warning disable IL2026
				path.AddPath(childPath);
			}

			return path;
		}

		static SKPath MakePath(PathGeometry pathGeometry)
		{
			var path = new SKPath();
			path.FillType = pathGeometry.FillRule == FillRule.Nonzero ? SKPathFillType.Winding : SKPathFillType.EvenOdd;

			foreach (PathFigure pathFigure in pathGeometry.Figures)
			{
				path.MoveTo(
					Forms.ConvertToScaledPixel(pathFigure.StartPoint.X),
					Forms.ConvertToScaledPixel(pathFigure.StartPoint.Y));

				Point lastPoint = pathFigure.StartPoint;

				foreach (PathSegment pathSegment in pathFigure.Segments)
				{
					// LineSegment
					if (pathSegment is LineSegment)
					{
						LineSegment lineSegment = pathSegment as LineSegment;

						path.LineTo(
							Forms.ConvertToScaledPixel(lineSegment.Point.X),
							Forms.ConvertToScaledPixel(lineSegment.Point.Y));
						lastPoint = lineSegment.Point;
					}
					// PolylineSegment
					else if (pathSegment is PolyLineSegment)
					{
						PolyLineSegment polylineSegment = pathSegment as PolyLineSegment;
						PointCollection points = polylineSegment.Points;

						for (int i = 0; i < points.Count; i++)
						{
							path.LineTo(
								Forms.ConvertToScaledPixel(points[i].X),
								Forms.ConvertToScaledPixel(points[i].Y));
						}
						lastPoint = points[points.Count - 1];
					}
					// BezierSegment
					else if (pathSegment is BezierSegment)
					{
						BezierSegment bezierSegment = pathSegment as BezierSegment;

						path.CubicTo(
							Forms.ConvertToScaledPixel(bezierSegment.Point1.X), Forms.ConvertToScaledPixel(bezierSegment.Point1.Y),
							Forms.ConvertToScaledPixel(bezierSegment.Point2.X), Forms.ConvertToScaledPixel(bezierSegment.Point2.Y),
							Forms.ConvertToScaledPixel(bezierSegment.Point3.X), Forms.ConvertToScaledPixel(bezierSegment.Point3.Y));

						lastPoint = bezierSegment.Point3;
					}
					// PolyBezierSegment
					else if (pathSegment is PolyBezierSegment)
					{
						PolyBezierSegment polyBezierSegment = pathSegment as PolyBezierSegment;
						PointCollection points = polyBezierSegment.Points;

						for (int i = 0; i < points.Count; i += 3)
						{
							path.CubicTo(
								Forms.ConvertToScaledPixel(points[i + 0].X), Forms.ConvertToScaledPixel(points[i + 0].Y),
								Forms.ConvertToScaledPixel(points[i + 1].X), Forms.ConvertToScaledPixel(points[i + 1].Y),
								Forms.ConvertToScaledPixel(points[i + 2].X), Forms.ConvertToScaledPixel(points[i + 2].Y));
						}

						lastPoint = points[points.Count - 1];
					}
					// QuadraticBezierSegment
					else if (pathSegment is QuadraticBezierSegment)
					{
						QuadraticBezierSegment bezierSegment = pathSegment as QuadraticBezierSegment;

						path.QuadTo(
							Forms.ConvertToScaledPixel(bezierSegment.Point1.X), Forms.ConvertToScaledPixel(bezierSegment.Point1.Y),
							Forms.ConvertToScaledPixel(bezierSegment.Point2.X), Forms.ConvertToScaledPixel(bezierSegment.Point2.Y));

						lastPoint = bezierSegment.Point2;
					}
					// PolyQuadraticBezierSegment
					else if (pathSegment is PolyQuadraticBezierSegment)
					{
						PolyQuadraticBezierSegment polyBezierSegment = pathSegment as PolyQuadraticBezierSegment;
						PointCollection points = polyBezierSegment.Points;

						for (int i = 0; i < points.Count; i += 2)
						{
							path.QuadTo(
								Forms.ConvertToScaledPixel(points[i + 0].X), Forms.ConvertToScaledPixel(points[i + 0].Y),
								Forms.ConvertToScaledPixel(points[i + 1].X), Forms.ConvertToScaledPixel(points[i + 1].Y));
						}

						lastPoint = points[points.Count - 1];
					}
					// ArcSegment
					else if (pathSegment is ArcSegment)
					{
						ArcSegment arcSegment = pathSegment as ArcSegment;

						List<Point> points = new List<Point>();

						GeometryHelper.FlattenArc(points,
							lastPoint,
							arcSegment.Point,
							arcSegment.Size.Width,
							arcSegment.Size.Height,
							arcSegment.RotationAngle,
							arcSegment.IsLargeArc,
							arcSegment.SweepDirection == SweepDirection.CounterClockwise,
							1);

						for (int i = 0; i < points.Count; i++)
						{
							path.LineTo(
								Forms.ConvertToScaledPixel(points[i].X),
								Forms.ConvertToScaledPixel(points[i].Y));
						}

						if (points.Count > 0)
							lastPoint = points[points.Count - 1];
					}
				}

				if (pathFigure.IsClosed)
					path.Close();
			}

			return path;
		}
	}
}
