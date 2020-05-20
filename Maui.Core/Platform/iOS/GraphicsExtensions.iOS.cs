using System;
using System.Maui.Graphics;
using CoreGraphics;

namespace System.Maui
{
	public static class CoreGraphicsExtensions
	{
		//public static Point ToPointF(this CGPoint size)
		//{
		//	return new Point(size.X, size.Y);
		//}

		//public static Size ToSizeF(this CGSize size)
		//{
		//	return new Size(size.Width, size.Height);
		//}

		//public static CGSize ToCGSize(this Size size)
		//{
		//	return new CGSize(size.Width, size.Height);
		//}

		//public static Rectangle ToRectangleF(this CGRect rect)
		//{
		//	return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		//}

		//public static CGRect ToCGRect(this Rectangle rect)
		//{
		//	return new CGRect(rect.X, rect.Y, rect.Width, rect.Height);
		//}

		public static CGPath ToCGPath(
			this Path target)
		{
			var path = new CGPath();

			int pointIndex = 0;
			int arcAngleIndex = 0;
			int arcClockwiseIndex = 0;

			foreach (var operation in target.PathOperations)
			{
				if (operation == PathOperation.MoveTo)
				{
					var point = target[pointIndex++];
					path.MoveToPoint((nfloat)point.X, (nfloat)point.Y);
				}
				else if (operation == PathOperation.Line)
				{
					var endPoint = target[pointIndex++];
					path.AddLineToPoint((nfloat)endPoint.X, (nfloat)endPoint.Y);

				}

				else if (operation == PathOperation.Quad)
				{
					var controlPoint = target[pointIndex++];
					var endPoint = target[pointIndex++];
					path.AddQuadCurveToPoint(
						(nfloat)controlPoint.X,
						(nfloat)controlPoint.Y,
						(nfloat)endPoint.X,
						(nfloat)endPoint.Y);
				}
				else if (operation == PathOperation.Cubic)
				{
					var controlPoint1 = target[pointIndex++];
					var controlPoint2 = target[pointIndex++];
					var endPoint = target[pointIndex++];
					path.AddCurveToPoint(
						(nfloat)controlPoint1.X,
						(nfloat)controlPoint1.Y,
						(nfloat)controlPoint2.X,
						(nfloat)controlPoint2.Y,
						(nfloat)endPoint.X,
						(nfloat)endPoint.Y);
				}
				else if (operation == PathOperation.Arc)
				{
					var topLeft = target[pointIndex++];
					var bottomRight = target[pointIndex++];
					var startAngle = target.GetArcAngle(arcAngleIndex++);
					var endAngle = target.GetArcAngle(arcAngleIndex++);
					var clockwise = target.IsArcClockwise(arcClockwiseIndex++);

					var startAngleInRadians = GraphicsOperations.DegreesToRadians(-startAngle);
					var endAngleInRadians = GraphicsOperations.DegreesToRadians(-endAngle);

					while (startAngleInRadians < 0)
					{
						startAngleInRadians += Math.PI * 2;
					}

					while (endAngleInRadians < 0)
					{
						endAngleInRadians += Math.PI * 2;
					}

					var cx = (bottomRight.X + topLeft.X) / 2;
					var cy = (bottomRight.Y + topLeft.Y) / 2;
					var width = bottomRight.X - topLeft.X;
					var height = bottomRight.Y - topLeft.Y;
					var r = width / 2;

					var transform = CGAffineTransform.MakeTranslation((nfloat)cx, (nfloat)cy);
					transform = CGAffineTransform.Multiply(CGAffineTransform.MakeScale(1, (nfloat)height / (nfloat)width), transform);

					path.AddArc(transform, 0, 0, (nfloat)r, (nfloat)startAngleInRadians, (nfloat)endAngleInRadians, !clockwise);
				}
				else if (operation == PathOperation.Close)
				{
					path.CloseSubpath();
				}
			}

			return path;
		}
	}
}
