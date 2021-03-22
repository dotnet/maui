using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Microsoft.Maui.Graphics;
using APath = Android.Graphics.Path;

namespace Microsoft.Maui
{
	public static class GeometryExtensions
	{
		public static APath ToNative(this IGeometry geometry, Context context)
		{
			APath path = new APath();

			float density = context.Resources?.DisplayMetrics?.Density ?? 1.0f;

			if (geometry is LineGeometry lineGeometry)
			{
				path.MoveTo(
					density * (float)lineGeometry.StartPoint.X,
					density * (float)lineGeometry.StartPoint.Y);

				path.LineTo(
					density * (float)lineGeometry.EndPoint.X,
					density * (float)lineGeometry.EndPoint.Y);
			}
			else if (geometry is RectangleGeometry rectangleGeometry)
			{
				Rect rect = rectangleGeometry.Rect;

				path.AddRect(
					density * (float)rect.Left,
					density * (float)rect.Top,
					density * (float)rect.Right,
					density * (float)rect.Bottom,
					APath.Direction.Cw!);
			}
			else if (geometry is EllipseGeometry ellipseGeometry)
			{
				path.AddOval(new RectF(
					density * (float)(ellipseGeometry.Center.X - ellipseGeometry.RadiusX),
					density * (float)(ellipseGeometry.Center.Y - ellipseGeometry.RadiusY),
					density * (float)(ellipseGeometry.Center.X + ellipseGeometry.RadiusX),
					density * (float)(ellipseGeometry.Center.Y + ellipseGeometry.RadiusY)),
					APath.Direction.Cw!);
			}
			else if (geometry is GeometryGroup geometryGroup)
			{
				path.SetFillType(geometryGroup.FillRule == FillRule.Nonzero ? APath.FillType.Winding! : APath.FillType.EvenOdd!);

				foreach (Geometry child in geometryGroup.Children)
				{
					APath childPath = child.ToNative(context);
					path.AddPath(childPath);
				}
			}
			else if (geometry is PathGeometry pathGeometry)
			{
				path.SetFillType(pathGeometry.FillRule == FillRule.Nonzero ? APath.FillType.Winding! : APath.FillType.EvenOdd!);

				foreach (PathFigure pathFigure in pathGeometry.Figures)
				{
					path.MoveTo(
						density * (float)pathFigure.StartPoint.X,
						density * (float)pathFigure.StartPoint.Y);

					Point lastPoint = pathFigure.StartPoint;

					foreach (PathSegment pathSegment in pathFigure.Segments)
					{
						// LineSegment
						if (pathSegment is LineSegment lineSegment)
						{
							path.LineTo(
								density * (float)lineSegment.Point.X,
								density * (float)lineSegment.Point.Y);
							lastPoint = lineSegment.Point;
						}
						// PolylineSegment
						else if (pathSegment is PolyLineSegment polylineSegment)
						{
							PointCollection points = polylineSegment.Points;

							for (int i = 0; i < points.Count; i++)
							{
								path.LineTo(
									density * (float)points[i].X,
									density * (float)points[i].Y);
							}
							lastPoint = points[^1];
						}
						// BezierSegment
						else if (pathSegment is BezierSegment bezierSegment)
						{
							path.CubicTo(
								density * (float)bezierSegment.Point1.X, density * (float)bezierSegment.Point1.Y,
								density * (float)bezierSegment.Point2.X, density * (float)bezierSegment.Point2.Y,
								density * (float)bezierSegment.Point3.X, density * (float)bezierSegment.Point3.Y);

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
									path.CubicTo(
										density * (float)points[i + 0].X, density * (float)points[i + 0].Y,
										density * (float)points[i + 1].X, density * (float)points[i + 1].Y,
										density * (float)points[i + 2].X, density * (float)points[i + 2].Y);
								}
							}

							lastPoint = points[^1];
						}
						// QuadraticBezierSegment
						else if (pathSegment is QuadraticBezierSegment quadraticBezierSegment)
						{

							path.QuadTo(
								density * (float)quadraticBezierSegment.Point1.X, density * (float)quadraticBezierSegment.Point1.Y,
								density * (float)quadraticBezierSegment.Point2.X, density * (float)quadraticBezierSegment.Point2.Y);

							lastPoint = quadraticBezierSegment.Point2;
						}
						// PolyQuadraticBezierSegment
						else if (pathSegment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
						{
							PointCollection points = polyQuadraticBezierSegment.Points;

							if (points.Count >= 2)
							{
								for (int i = 0; i < points.Count; i += 2)
								{
									path.QuadTo(
										density * (float)points[i + 0].X, density * (float)points[i + 0].Y,
										density * (float)points[i + 1].X, density * (float)points[i + 1].Y);
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

							for (int i = 0; i < points.Count; i++)
							{
								path.LineTo(
									density * (float)points[i].X,
									density * (float)points[i].Y);
							}

							if (points.Count > 0)
								lastPoint = points[^1];
						}
					}

					if (pathFigure.IsClosed)
						path.Close();
				}
			}

			return path;
		}
	}
}