using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

namespace Microsoft.Maui.Graphics.Xaml
{
	public static class XamlGraphicsExtensions
	{
		public static System.Windows.Media.Color AsWpfColor(this Color target)
		{
			return System.Windows.Media.Color.FromArgb(
				(byte) (255 * target.Alpha),
				(byte) (255 * target.Red),
				(byte) (255 * target.Green),
				(byte) (255 * target.Blue));
		}

		public static PointF AsPointF(this global::System.Windows.Point point)
		{
			return new PointF((float) point.X, (float) point.Y);
		}

		public static global::System.Windows.Point AsPoint(this PointF target)
		{
			return new global::System.Windows.Point(target.X, target.Y);
		}

		public static global::System.Windows.Point AsPoint(this PointF target, float ppu)
		{
			return new global::System.Windows.Point(target.X * ppu, target.Y * ppu);
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public static PathGeometry AsPathGeometry(this PathF target, float scale = 1)
		{
			var geometry = new PathGeometry();
			PathFigure figure = null;

			var pointIndex = 0;
			var arcAngleIndex = 0;
			var arcClockwiseIndex = 0;

			foreach (var type in target.SegmentTypes)
			{
				if (type == PathOperation.Move)
				{
					figure = new PathFigure();
					geometry.Figures.Add(figure);
					figure.StartPoint = target[pointIndex++].AsPoint(scale);
				}
				else if (type == PathOperation.Line)
				{
					var lineSegment = new LineSegment {Point = target[pointIndex++].AsPoint(scale)};
					figure.Segments.Add(lineSegment);
				}
				else if (type == PathOperation.Quad)
				{
					var quadSegment = new QuadraticBezierSegment
					{
						Point1 = target[pointIndex++].AsPoint(scale),
						Point2 = target[pointIndex++].AsPoint(scale)
					};
					figure.Segments.Add(quadSegment);
				}
				else if (type == PathOperation.Cubic)
				{
					var cubicSegment = new BezierSegment()
					{
						Point1 = target[pointIndex++].AsPoint(scale),
						Point2 = target[pointIndex++].AsPoint(scale),
						Point3 = target[pointIndex++].AsPoint(scale),
					};
					figure.Segments.Add(cubicSegment);
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

					var sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);
					var absSweep = Math.Abs(sweep);

					var rectX = topLeft.X * scale;
					var rectY = topLeft.Y * scale;
					var rectWidth = bottomRight.X * scale - topLeft.X * scale;
					var rectHeight = bottomRight.Y * scale - topLeft.Y * scale;

					var startPoint = Geometry.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -startAngle);
					var endPoint = Geometry.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -endAngle);

					if (figure == null)
					{
						figure = new PathFigure();
						geometry.Figures.Add(figure);
						figure.StartPoint = startPoint.AsPoint();
					}
					else
					{
						var lineSegment = new LineSegment()
						{
							Point = startPoint.AsPoint()
						};
						figure.Segments.Add(lineSegment);
					}

					var arcSegment = new ArcSegment()
					{
						Point = new global::System.Windows.Point(endPoint.X, endPoint.Y),
						Size = new global::System.Windows.Size(rectWidth / 2, rectHeight / 2),
						SweepDirection = clockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
						IsLargeArc = absSweep >= 180,
					};
					figure.Segments.Add(arcSegment);
				}
				else if (type == PathOperation.Close)
				{
					figure.IsClosed = true;
				}
			}

			return geometry;
		}

		public static Transform AsTransform(this AffineTransform transform)
		{
			if (transform.IsIdentity)
			{
				return Transform.Identity;
			}

			var values = new float[6];
			transform.GetMatrix(values);
			return new MatrixTransform(values[0], values[1], values[2], values[3], values[4], values[5]);
		}
	}
}
