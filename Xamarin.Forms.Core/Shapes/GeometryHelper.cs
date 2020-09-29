using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Shapes
{
	public static class GeometryHelper
	{
		static readonly List<Point> Points = new List<Point>();

		public static PathGeometry FlattenGeometry(Geometry geoSrc, double tolerance)
		{
			// Return empty PathGeometry if Geometry is null
			if (geoSrc == null)
				return new PathGeometry();

			PathGeometry pathGeoDst = new PathGeometry();
			FlattenGeometry(pathGeoDst, geoSrc, tolerance, Matrix.Identity);

			return pathGeoDst;
		}

		public static void FlattenGeometry(PathGeometry pathGeoDst, Geometry geoSrc, double tolerance, Matrix matxPrevious)
		{
			Matrix matx = matxPrevious;

			if (geoSrc is GeometryGroup)
			{
				foreach (Geometry geoChild in (geoSrc as GeometryGroup).Children)
				{
					FlattenGeometry(pathGeoDst, geoChild, tolerance, matx);
				}
			}
			else if (geoSrc is LineGeometry)
			{
				LineGeometry lineGeoSrc = geoSrc as LineGeometry;
				PathFigure figDst = new PathFigure();
				PolyLineSegment segDst = new PolyLineSegment();

				figDst.StartPoint = matx.Transform(lineGeoSrc.StartPoint);
				segDst.Points.Add(matx.Transform(lineGeoSrc.EndPoint));

				figDst.Segments.Add(segDst);
				pathGeoDst.Figures.Add(figDst);
			}
			else if (geoSrc is RectangleGeometry)
			{
				RectangleGeometry rectGeoSrc = geoSrc as RectangleGeometry;
				PathFigure figDst = new PathFigure();
				PolyLineSegment segDst = new PolyLineSegment();

				figDst.StartPoint = matx.Transform(new Point(rectGeoSrc.Rect.Left, rectGeoSrc.Rect.Top));
				segDst.Points.Add(matx.Transform(new Point(rectGeoSrc.Rect.Right, rectGeoSrc.Rect.Top)));
				segDst.Points.Add(matx.Transform(new Point(rectGeoSrc.Rect.Right, rectGeoSrc.Rect.Bottom)));
				segDst.Points.Add(matx.Transform(new Point(rectGeoSrc.Rect.Left, rectGeoSrc.Rect.Bottom)));
				segDst.Points.Add(matx.Transform(new Point(rectGeoSrc.Rect.Left, rectGeoSrc.Rect.Top)));

				figDst.IsClosed = true;
				figDst.Segments.Add(segDst);
				pathGeoDst.Figures.Add(figDst);
			}
			else if (geoSrc is EllipseGeometry)
			{
				EllipseGeometry elipGeoSrc = geoSrc as EllipseGeometry;
				PathFigure figDst = new PathFigure();
				PolyLineSegment segDst = new PolyLineSegment();

				int max = (int)(4 * (elipGeoSrc.RadiusX + elipGeoSrc.RadiusY) / tolerance);

				for (int i = 0; i < max; i++)
				{
					double x = elipGeoSrc.Center.X + elipGeoSrc.RadiusX * Math.Sin(i * 2 * Math.PI / max);
					double y = elipGeoSrc.Center.Y - elipGeoSrc.RadiusY * Math.Cos(i * 2 * Math.PI / max);
					Point pt = matx.Transform(new Point(x, y));

					if (i == 0)
						figDst.StartPoint = pt;
					else
						segDst.Points.Add(pt);
				}

				figDst.IsClosed = true;
				figDst.Segments.Add(segDst);
				pathGeoDst.Figures.Add(figDst);
			}
			else if (geoSrc is PathGeometry)
			{
				PathGeometry pathGeoSrc = geoSrc as PathGeometry;
				pathGeoDst.FillRule = pathGeoSrc.FillRule;

				foreach (PathFigure figSrc in pathGeoSrc.Figures)
				{
					PathFigure figDst = new PathFigure
					{
						IsFilled = figSrc.IsFilled,
						IsClosed = figSrc.IsClosed,
						StartPoint = matx.Transform(figSrc.StartPoint)
					};
					Point ptLast = figDst.StartPoint;

					foreach (PathSegment segSrc in figSrc.Segments)
					{
						PolyLineSegment segDst = new PolyLineSegment();

						if (segSrc is LineSegment)
						{
							LineSegment lineSegSrc = segSrc as LineSegment;
							ptLast = matx.Transform(lineSegSrc.Point);
							segDst.Points.Add(ptLast);
						}
						else if (segSrc is PolyLineSegment)
						{
							PolyLineSegment polySegSrc = segSrc as PolyLineSegment;

							foreach (Point pt in polySegSrc.Points)
							{
								ptLast = matx.Transform(pt);
								segDst.Points.Add(ptLast);
							}
						}
						else if (segSrc is BezierSegment)
						{
							BezierSegment bezSeg = segSrc as BezierSegment;
							Point pt0 = ptLast;
							Point pt1 = matx.Transform(bezSeg.Point1);
							Point pt2 = matx.Transform(bezSeg.Point2);
							Point pt3 = matx.Transform(bezSeg.Point3);

							Points.Clear();
							FlattenCubicBezier(Points, pt0, pt1, pt2, pt3, tolerance);

							for (int i = 1; i < Points.Count; i++)
								segDst.Points.Add(Points[i]);

							ptLast = Points[Points.Count - 1];
						}
						else if (segSrc is PolyBezierSegment)
						{
							PolyBezierSegment polyBezSeg = segSrc as PolyBezierSegment;

							for (int bez = 0; bez < polyBezSeg.Points.Count; bez += 3)
							{
								if (bez + 2 > polyBezSeg.Points.Count - 1)
									break;

								Point pt0 = ptLast;
								Point pt1 = matx.Transform(polyBezSeg.Points[bez]);
								Point pt2 = matx.Transform(polyBezSeg.Points[bez + 1]);
								Point pt3 = matx.Transform(polyBezSeg.Points[bez + 2]);

								Points.Clear();
								FlattenCubicBezier(Points, pt0, pt1, pt2, pt3, tolerance);

								for (int i = 1; i < Points.Count; i++)
									segDst.Points.Add(Points[i]);

								ptLast = Points[Points.Count - 1];
							}
						}
						else if (segSrc is QuadraticBezierSegment)
						{
							QuadraticBezierSegment quadBezSeg = segSrc as QuadraticBezierSegment;
							Point pt0 = ptLast;
							Point pt1 = matx.Transform(quadBezSeg.Point1);
							Point pt2 = matx.Transform(quadBezSeg.Point2);

							Points.Clear();
							FlattenQuadraticBezier(Points, pt0, pt1, pt2, tolerance);

							for (int i = 1; i < Points.Count; i++)
								segDst.Points.Add(Points[i]);

							ptLast = Points[Points.Count - 1];
						}
						else if (segSrc is PolyQuadraticBezierSegment)
						{
							PolyQuadraticBezierSegment polyQuadBezSeg = segSrc as PolyQuadraticBezierSegment;

							for (int bez = 0; bez < polyQuadBezSeg.Points.Count; bez += 2)
							{
								if (bez + 1 > polyQuadBezSeg.Points.Count - 1)
									break;

								Point pt0 = ptLast;
								Point pt1 = matx.Transform(polyQuadBezSeg.Points[bez]);
								Point pt2 = matx.Transform(polyQuadBezSeg.Points[bez + 1]);

								Points.Clear();
								FlattenQuadraticBezier(Points, pt0, pt1, pt2, tolerance);

								for (int i = 1; i < Points.Count; i++)
									segDst.Points.Add(Points[i]);

								ptLast = Points[Points.Count - 1];
							}
						}
						else if (segSrc is ArcSegment)
						{
							ArcSegment arcSeg = segSrc as ArcSegment;

							Points.Clear();

							FlattenArc(
								Points,
								ptLast,
								arcSeg.Point,
								arcSeg.Size.Width,
								arcSeg.Size.Height,
								arcSeg.RotationAngle,
								arcSeg.IsLargeArc,
								arcSeg.SweepDirection == SweepDirection.CounterClockwise,
								tolerance);

							// Set ptLast while transferring points
							for (int i = 1; i < Points.Count; i++)
								segDst.Points.Add(ptLast = Points[i]);
						}

						figDst.Segments.Add(segDst);
					}

					pathGeoDst.Figures.Add(figDst);
				}
			}
		}

		public static void FlattenCubicBezier(List<Point> points, Point ptStart, Point ptCtrl1, Point ptCtrl2, Point ptEnd, double tolerance)
		{
			int max = (int)((ptCtrl1.Distance(ptStart) + ptCtrl2.Distance(ptCtrl1) + ptEnd.Distance(ptCtrl2)) / tolerance);

			for (int i = 0; i <= max; i++)
			{
				double t = (double)i / max;

				double x = (1 - t) * (1 - t) * (1 - t) * ptStart.X +
						   3 * t * (1 - t) * (1 - t) * ptCtrl1.X +
						   3 * t * t * (1 - t) * ptCtrl2.X +
						   t * t * t * ptEnd.X;

				double y = (1 - t) * (1 - t) * (1 - t) * ptStart.Y +
						   3 * t * (1 - t) * (1 - t) * ptCtrl1.Y +
						   3 * t * t * (1 - t) * ptCtrl2.Y +
						   t * t * t * ptEnd.Y;

				points.Add(new Point(x, y));
			}
		}

		public static void FlattenQuadraticBezier(List<Point> points, Point ptStart, Point ptCtrl, Point ptEnd, double tolerance)
		{
			int max = (int)((ptCtrl.Distance(ptStart) + ptEnd.Distance(ptCtrl)) / tolerance);

			for (int i = 0; i <= max; i++)
			{
				double t = (double)i / max;

				double x = (1 - t) * (1 - t) * ptStart.X +
						   2 * t * (1 - t) * ptCtrl.X +
						   t * t * ptEnd.X;

				double y = (1 - t) * (1 - t) * ptStart.Y +
						   2 * t * (1 - t) * ptCtrl.Y +
						   t * t * ptEnd.Y;

				points.Add(new Point(x, y));
			}
		}

		// More information: http://www.charlespetzold.com/blog/2008/01/Mathematics-of-ArcSegment.html
		public static void FlattenArc(List<Point> points, Point pt1, Point pt2, double radiusX, double radiusY, double angleRotation,
			bool isLargeArc, bool isCounterclockwise, double tolerance)
		{
			// Adjust for different radii and rotation angle
			Matrix matx = new Matrix();
			matx.Rotate(-angleRotation);
			matx.Scale(radiusY / radiusX, 1);
			pt1 = matx.Transform(pt1);
			pt2 = matx.Transform(pt2);

			// Get info about chord that connects both points
			Point midPoint = new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
			Vector2 vect = new Vector2(pt2.X - pt1.X, pt2.Y - pt1.Y);
			double halfChord = vect.Length / 2;

			// Get vector from chord to center
			Vector2 vectRotated;

			if (isLargeArc == isCounterclockwise)
				vectRotated = new Vector2(-vect.Y, vect.X);
			else
				vectRotated = new Vector2(vect.Y, -vect.X);

			vectRotated = vectRotated.Normalized;

			// Distance from chord to center
			double centerDistance = Math.Sqrt(Math.Abs((radiusY * radiusY) - (halfChord * halfChord)));

			// Calculate center point
			Point center = midPoint + centerDistance * vectRotated;

			// Get angles from center to the two points
			double angle1 = Math.Atan2(pt1.Y - center.Y, pt1.X - center.X);
			double angle2 = Math.Atan2(pt2.Y - center.Y, pt2.X - center.X);

			double sweep = Math.Abs(angle2 - angle1);
			bool reverseArc;

			if (Math.IEEERemainder(sweep + 0.000005, Math.PI) < 0.000010)
			{
				reverseArc = isCounterclockwise == angle1 < angle2;
			}
			else
			{
				bool isAcute = sweep < Math.PI;
				reverseArc = isLargeArc == isAcute;
			}

			if (reverseArc)
			{
				if (angle1 < angle2)
					angle1 += 2 * Math.PI;
				else
					angle2 += 2 * Math.PI;
			}

			// Invert matrix for final point calculation
			matx.Invert();

			// Calculate number of points for polyline approximation
			int max = (int)(4 * (radiusX + radiusY) * Math.Abs(angle2 - angle1) / (2 * Math.PI) / tolerance);

			for (int i = 0; i <= max; i++)
			{
				double angle = ((max - i) * angle1 + i * angle2) / max;
				double x = center.X + radiusY * Math.Cos(angle);
				double y = center.Y + radiusY * Math.Sin(angle);

				Point pt = matx.Transform(new Point(x, y));
				points.Add(pt);
			}
		}
	}
}