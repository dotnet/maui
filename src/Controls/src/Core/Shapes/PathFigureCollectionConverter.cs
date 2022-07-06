using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PathFigureCollectionConverter']/Docs" />
	public class PathFigureCollectionConverter : TypeConverter
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> false;

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			PathFigureCollection pathFigureCollection = new PathFigureCollection();

			ParseStringToPathFigureCollection(pathFigureCollection, strValue);

			return pathFigureCollection;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			=> throw new NotSupportedException();

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="//Member[@MemberName='ParseStringToPathFigureCollection']/Docs" />
		public static void ParseStringToPathFigureCollection(PathFigureCollection pathFigureCollection, string pathString)
		{
			if (pathString != null)
			{
				var geometry = GetPathGeometry(pathString);
				pathFigureCollection = geometry.Figures;
			}
		}
		
		internal static PathGeometry GetPathGeometry(string pathString)
		{
			var pathBuilder = new PathBuilder();
			var path = pathBuilder.BuildPath(pathString);
	
			var geometry = new PathGeometry();
			PathFigure figure = null;

			var pointIndex = 0;
			var arcAngleIndex = 0;
			var arcClockwiseIndex = 0;

			foreach (var type in path.SegmentTypes)
			{
				if (type == PathOperation.Move)
				{
					figure = new PathFigure();
					geometry.Figures.Add(figure);
					figure.StartPoint = path[pointIndex++];
				}
				else if (type == PathOperation.Line)
				{
					var lineSegment = new LineSegment { Point = path[pointIndex++] };
					figure.Segments.Add(lineSegment);
				}
				else if (type == PathOperation.Quad)
				{
					var quadSegment = new QuadraticBezierSegment
					{
						Point1 = path[pointIndex++],
						Point2 = path[pointIndex++]
					};
					figure.Segments.Add(quadSegment);
				}
				else if (type == PathOperation.Cubic)
				{
					var cubicSegment = new BezierSegment()
					{
						Point1 = path[pointIndex++],
						Point2 = path[pointIndex++],
						Point3 = path[pointIndex++],
					};
					figure.Segments.Add(cubicSegment);
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

					var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);
					var absSweep = Math.Abs(sweep);

					var rectX = topLeft.X;
					var rectY = topLeft.Y;
					var rectWidth = bottomRight.X  - topLeft.X;
					var rectHeight = bottomRight.Y  - topLeft.Y;

					var startPoint = GeometryUtil.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -startAngle);
					var endPoint = GeometryUtil.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -endAngle);

					if (figure == null)
					{
						figure = new PathFigure();
						geometry.Figures.Add(figure);
						figure.StartPoint = startPoint;
					}
					else
					{
						var lineSegment = new LineSegment()
						{
							Point = startPoint
						};
						figure.Segments.Add(lineSegment);
					}

					var arcSegment = new ArcSegment()
					{
						Point = new Point(endPoint.X, endPoint.Y),
						Size = new Size(rectWidth / 2, rectHeight / 2),
						SweepDirection = clockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
						IsLargeArc = absSweep >= 180
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
	}
}