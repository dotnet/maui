using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace Microsoft.Maui.Graphics.Win2D
{
	public static class W2DExtensions
	{
		public static global::Windows.UI.Color AsColor(this Color color, Color defaultColor, float alpha = 1)
		{
			var finalColor = color ?? defaultColor;

			var r = (byte)(finalColor.Red * 255);
			var g = (byte)(finalColor.Green * 255);
			var b = (byte)(finalColor.Blue * 255);
			var a = (byte)(finalColor.Alpha * 255 * alpha);

			return global::Windows.UI.Color.FromArgb(a, r, g, b);
		}

		public static global::Windows.UI.Color AsColor(this Color color, float alpha = 1)
		{
			var r = (byte)(color.Red * 255);
			var g = (byte)(color.Green * 255);
			var b = (byte)(color.Blue * 255);
			var a = (byte)(color.Alpha * 255 * alpha);

			return global::Windows.UI.Color.FromArgb(a, r, g, b);
		}

		public static Matrix3x2 Scale(this Matrix3x2 target, float sx, float sy)
		{
			return Matrix3x2.Multiply(Matrix3x2.CreateScale(sx, sy), target);
			/* target.M11 *= sx;
            target.M22 *= sy;
            return target;*/
		}

		public static Matrix3x2 Translate(this Matrix3x2 target, float dx, float dy)
		{
			return Matrix3x2.Multiply(Matrix3x2.CreateTranslation(dx, dy), target);
			/*target.M31 += dx;
            target.M32 += dy;
            return target;*/
		}

		public static Matrix3x2 Rotate(this Matrix3x2 target, float radians)
		{
			Matrix3x2 vMatrix = Matrix3x2.Multiply(Matrix3x2.CreateRotation(radians), target);
			/* target.M31 += dx;
            target.M32 += dy;*/
			return vMatrix;
		}

		public static CanvasGeometry AsPath(this PathF path, ICanvasResourceCreator creator, CanvasFilledRegionDetermination fillMode = CanvasFilledRegionDetermination.Winding)
		{
			return AsPath(path, 0, 0, 1, 1, creator, fillMode);
		}

		public static CanvasGeometry AsPath(this PathF path, float ox, float oy, float fx, float fy, ICanvasResourceCreator creator, CanvasFilledRegionDetermination fillMode = CanvasFilledRegionDetermination.Winding)
		{
			var builder = new CanvasPathBuilder(creator);

#if DEBUG
			try
			{
#endif
				builder.SetFilledRegionDetermination(fillMode);

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
							builder.EndFigure(CanvasFigureLoop.Open);
						}

						var point = path[pointIndex++];
						var begin = CanvasFigureFill.Default;
						builder.BeginFigure(ox + point.X * fx, oy + point.Y * fy, begin);
						figureOpen = true;
					}
					else if (type == PathOperation.Line)
					{
						var point = path[pointIndex++];
						builder.AddLine(ox + point.X * fx, oy + point.Y * fy);
					}

					else if (type == PathOperation.Quad)
					{
						var controlPoint = path[pointIndex++];
						var endPoint = path[pointIndex++];

						builder.AddQuadraticBezier(
							new Vector2(ox + controlPoint.X * fx, oy + controlPoint.Y * fy),
							new Vector2(ox + endPoint.X * fx, oy + endPoint.Y * fy));
					}
					else if (type == PathOperation.Cubic)
					{
						var controlPoint1 = path[pointIndex++];
						var controlPoint2 = path[pointIndex++];
						var endPoint = path[pointIndex++];
						builder.AddCubicBezier(
							new Vector2(ox + controlPoint1.X * fx, oy + controlPoint1.Y * fy),
							new Vector2(ox + controlPoint2.X * fx, oy + controlPoint2.Y * fy),
							new Vector2(ox + endPoint.X * fx, oy + endPoint.Y * fy));
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

						var rotation = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);
						var absRotation = Math.Abs(rotation);

						var rectX = ox + topLeft.X * fx;
						var rectY = oy + topLeft.Y * fy;
						var rectWidth = (ox + bottomRight.X * fx) - rectX;
						var rectHeight = (oy + bottomRight.Y * fy) - rectY;

						var startPoint = GeometryUtil.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -startAngle);
						var endPoint = GeometryUtil.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -endAngle);


						if (!figureOpen)
						{
							var begin = CanvasFigureFill.Default;
							builder.BeginFigure(startPoint.X, startPoint.Y, begin);
							figureOpen = true;
						}
						else
						{
							builder.AddLine(startPoint.X, startPoint.Y);
						}

						builder.AddArc(
							 new Vector2(endPoint.X, endPoint.Y),
							 rectWidth / 2,
							 rectHeight / 2,
							 0,
							 clockwise ? CanvasSweepDirection.Clockwise : CanvasSweepDirection.CounterClockwise,
							 absRotation >= 180 ? CanvasArcSize.Large : CanvasArcSize.Small
							);
					}
					else if (type == PathOperation.Close)
					{
						builder.EndFigure(CanvasFigureLoop.Closed);
					}

					lastOperation = type;
				}

				if (segmentIndex >= 0 && lastOperation != PathOperation.Close)
				{
					builder.EndFigure(CanvasFigureLoop.Open);
				}

				var geometry = CanvasGeometry.CreatePath(builder);
				return geometry;
#if DEBUG
			}
			catch (Exception exc)
			{
				builder.Dispose();

				var definition = path.ToDefinitionString();
				System.Diagnostics.Debug.WriteLine("Unable to convert the path to a Win2D Path: {0}: {1}", definition, exc);
				return null;
			}
#endif
		}

		public static CanvasGeometry AsPathFromSegment(this PathF path, int segmentIndex, float zoom, ICanvasResourceCreator creator)
		{
			float scale = 1 / zoom;

			var builder = new CanvasPathBuilder(creator);

			var type = path.GetSegmentType(segmentIndex);
			if (type == PathOperation.Line)
			{
				int segmentStartingPointIndex = path.GetSegmentPointIndex(segmentIndex);
				var startPoint = path[segmentStartingPointIndex - 1];
				builder.BeginFigure(startPoint.X * scale, startPoint.Y * scale, CanvasFigureFill.Default);

				var point = path[segmentStartingPointIndex];
				builder.AddLine(point.X * scale, point.Y * scale);
			}
			else if (type == PathOperation.Quad)
			{
				int segmentStartingPointIndex = path.GetSegmentPointIndex(segmentIndex);
				var startPoint = path[segmentStartingPointIndex - 1];
				builder.BeginFigure(startPoint.X * scale, startPoint.Y * scale, CanvasFigureFill.Default);

				var controlPoint = path[segmentStartingPointIndex++];
				var endPoint = path[segmentStartingPointIndex];
				builder.AddQuadraticBezier(
					new Vector2(controlPoint.X * scale, controlPoint.Y * scale),
					new Vector2(endPoint.X * scale, endPoint.Y * scale));
			}
			else if (type == PathOperation.Cubic)
			{
				int segmentStartingPointIndex = path.GetSegmentPointIndex(segmentIndex);
				var startPoint = path[segmentStartingPointIndex - 1];
				builder.BeginFigure(startPoint.X * scale, startPoint.Y * scale, CanvasFigureFill.Default);

				var controlPoint1 = path[segmentStartingPointIndex++];
				var controlPoint2 = path[segmentStartingPointIndex++];
				var endPoint = path[segmentStartingPointIndex];
				builder.AddCubicBezier(
					new Vector2(controlPoint1.X * scale, controlPoint1.Y * scale),
					new Vector2(controlPoint2.X * scale, controlPoint2.Y * scale),
					new Vector2(endPoint.X * scale, endPoint.Y * scale));
			}
			else if (type == PathOperation.Arc)
			{
				path.GetSegmentInfo(segmentIndex, out var pointIndex, out var arcAngleIndex, out var arcClockwiseIndex);

				var topLeft = path[pointIndex++];
				var bottomRight = path[pointIndex];
				var startAngle = path.GetArcAngle(arcAngleIndex++);
				var endAngle = path.GetArcAngle(arcAngleIndex);
				var clockwise = path.GetArcClockwise(arcClockwiseIndex);

				while (startAngle < 0)
				{
					startAngle += 360;
				}

				while (endAngle < 0)
				{
					endAngle += 360;
				}

				var rotation = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);
				var absRotation = Math.Abs(rotation);

				var rectX = topLeft.X * scale;
				var rectY = topLeft.Y * scale;
				var rectWidth = (bottomRight.X * scale) - rectX;
				var rectHeight = (bottomRight.Y * scale) - rectY;

				var startPoint = GeometryUtil.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -startAngle);
				var endPoint = GeometryUtil.EllipseAngleToPoint(rectX, rectY, rectWidth, rectHeight, -endAngle);

				builder.BeginFigure(startPoint.X * scale, startPoint.Y * scale, CanvasFigureFill.Default);

				builder.AddArc(
							 new Vector2(endPoint.X, endPoint.Y),
							 rectWidth / 2,
							 rectHeight / 2,
							 0,
							 clockwise ? CanvasSweepDirection.Clockwise : CanvasSweepDirection.CounterClockwise,
							 absRotation >= 180 ? CanvasArcSize.Large : CanvasArcSize.Small
							);
			}

			builder.EndFigure(CanvasFigureLoop.Open);

			return CanvasGeometry.CreatePath(builder);
		}
	}
}
