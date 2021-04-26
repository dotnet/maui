using System;
using Microsoft.Maui.Controls.Shapes;
using WFoundation = Windows.Foundation;
using WMedia = Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Platform
{
	public static class GeometryExtensions
	{
		[Obsolete("ToWindows is obsolete. Please use ToNative instead")]
		public static WMedia.Geometry ToWindows(this Geometry geometry) =>
			geometry.ToNative();

		public static WMedia.Geometry ToNative(this Geometry geometry)
		{
			WMedia.Geometry wGeometry = null;

			if (geometry is LineGeometry)
			{
				LineGeometry lineGeometry = geometry as LineGeometry;
				wGeometry = new WMedia.LineGeometry
				{
					StartPoint = lineGeometry.StartPoint.ToNative(),
					EndPoint = lineGeometry.EndPoint.ToNative()
				};
			}
			else if (geometry is RectangleGeometry)
			{
				var rect = (geometry as RectangleGeometry).Rect;
				wGeometry = new WMedia.RectangleGeometry
				{
					Rect = new WFoundation.Rect(rect.X, rect.Y, rect.Width, rect.Height)
				};
			}
			else if (geometry is EllipseGeometry)
			{
				EllipseGeometry ellipseGeometry = geometry as EllipseGeometry;
				wGeometry = new WMedia.EllipseGeometry
				{
					Center = ellipseGeometry.Center.ToNative(),
					RadiusX = ellipseGeometry.RadiusX,
					RadiusY = ellipseGeometry.RadiusY
				};
			}
			else if (geometry is GeometryGroup)
			{
				GeometryGroup geometryGroup = geometry as GeometryGroup;
				wGeometry = new WMedia.GeometryGroup
				{
					FillRule = ConvertFillRule(geometryGroup.FillRule)
				};

				foreach (Geometry children in geometryGroup.Children)
				{
					WMedia.Geometry winChild = children.ToNative();
					(wGeometry as WMedia.GeometryGroup).Children.Add(winChild);
				}
			}
			else if (geometry is PathGeometry)
			{
				PathGeometry pathGeometry = geometry as PathGeometry;

				WMedia.PathGeometry wPathGeometry = new WMedia.PathGeometry
				{
					FillRule = ConvertFillRule(pathGeometry.FillRule)
				};

				foreach (PathFigure xamPathFigure in pathGeometry.Figures)
				{
					WMedia.PathFigure wPathFigure = new WMedia.PathFigure
					{
						StartPoint = xamPathFigure.StartPoint.ToNative(),
						IsFilled = xamPathFigure.IsFilled,
						IsClosed = xamPathFigure.IsClosed
					};
					wPathGeometry.Figures.Add(wPathFigure);

					foreach (PathSegment pathSegment in xamPathFigure.Segments)
					{
						// LineSegment
						if (pathSegment is LineSegment)
						{
							LineSegment lineSegment = pathSegment as LineSegment;

							WMedia.LineSegment winSegment = new WMedia.LineSegment
							{
								Point = lineSegment.Point.ToNative()
							};

							wPathFigure.Segments.Add(winSegment);
						}

						// PolylineSegment
						if (pathSegment is PolyLineSegment)
						{
							PolyLineSegment polyLineSegment = pathSegment as PolyLineSegment;
							WMedia.PolyLineSegment wSegment = new WMedia.PolyLineSegment();

							foreach (var point in polyLineSegment.Points)
							{
								wSegment.Points.Add(point.ToNative());
							}

							wPathFigure.Segments.Add(wSegment);
						}

						// BezierSegment
						if (pathSegment is BezierSegment)
						{
							BezierSegment bezierSegment = pathSegment as BezierSegment;

							WMedia.BezierSegment wSegment = new WMedia.BezierSegment
							{
								Point1 = bezierSegment.Point1.ToNative(),
								Point2 = bezierSegment.Point2.ToNative(),
								Point3 = bezierSegment.Point3.ToNative()
							};

							wPathFigure.Segments.Add(wSegment);
						}
						// PolyBezierSegment
						else if (pathSegment is PolyBezierSegment)
						{
							PolyBezierSegment polyBezierSegment = pathSegment as PolyBezierSegment;
							WMedia.PolyBezierSegment wSegment = new WMedia.PolyBezierSegment();

							foreach (var point in polyBezierSegment.Points)
							{
								wSegment.Points.Add(point.ToNative());
							}

							wPathFigure.Segments.Add(wSegment);
						}

						// QuadraticBezierSegment
						if (pathSegment is QuadraticBezierSegment)
						{
							QuadraticBezierSegment quadraticBezierSegment = pathSegment as QuadraticBezierSegment;

							WMedia.QuadraticBezierSegment wSegment = new WMedia.QuadraticBezierSegment
							{
								Point1 = quadraticBezierSegment.Point1.ToNative(),
								Point2 = quadraticBezierSegment.Point2.ToNative()
							};

							wPathFigure.Segments.Add(wSegment);
						}
						// PolyQuadraticBezierSegment
						else if (pathSegment is PolyQuadraticBezierSegment)
						{
							PolyQuadraticBezierSegment polyQuadraticBezierSegment = pathSegment as PolyQuadraticBezierSegment;
							WMedia.PolyQuadraticBezierSegment wSegment = new WMedia.PolyQuadraticBezierSegment();

							foreach (var point in polyQuadraticBezierSegment.Points)
							{
								wSegment.Points.Add(point.ToNative());
							}

							wPathFigure.Segments.Add(wSegment);
						}
						// ArcSegment
						else if (pathSegment is ArcSegment)
						{
							ArcSegment arcSegment = pathSegment as ArcSegment;

							WMedia.ArcSegment wSegment = new WMedia.ArcSegment
							{
								Size = new WFoundation.Size(arcSegment.Size.Width, arcSegment.Size.Height),
								RotationAngle = arcSegment.RotationAngle,
								IsLargeArc = arcSegment.IsLargeArc,
								SweepDirection = arcSegment.SweepDirection == SweepDirection.Clockwise ? WMedia.SweepDirection.Clockwise : WMedia.SweepDirection.Counterclockwise,
								Point = arcSegment.Point.ToNative()
							};

							wPathFigure.Segments.Add(wSegment);
						}
					}
				}

				wGeometry = wPathGeometry;
			}

			return wGeometry;
		}

		static WMedia.FillRule ConvertFillRule(FillRule fillRule)
		{
			return fillRule == FillRule.EvenOdd ? WMedia.FillRule.EvenOdd : WMedia.FillRule.Nonzero;
		}
	}
}