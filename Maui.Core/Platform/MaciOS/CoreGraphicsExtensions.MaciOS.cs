using CoreGraphics;

namespace System.Maui.Platform
{
	public static class CoreGraphicsExtensions
	{
		public static Point ToPoint(this CGPoint size)
		{
			return new Point((float)size.X, (float)size.Y);
		}

		public static Size ToSize(this CGSize size)
		{
			return new Size((float)size.Width, (float)size.Height);
		}

		public static CGSize ToCGSize(this Size size)
		{
			return new CGSize(size.Width, size.Height);
		}

		public static Rectangle ToRectangle(this CGRect rect)
		{
			return new Rectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
		}

		public static CGRect ToCGRect(this Rectangle rect)
		{
			return new CGRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		//public static CGPath ToCGPath(
		//	this PathF target)
		//{
		//	var path = new CGPath();

		//	int pointIndex = 0;
		//	int arcAngleIndex = 0;
		//	int arcClockwiseIndex = 0;

		//	foreach (var operation in target.PathOperations)
		//	{
		//		if (operation == PathOperation.MoveTo)
		//		{
		//			var point = target[pointIndex++];
		//			path.MoveToPoint(point.X, point.Y);
		//		}
		//		else if (operation == PathOperation.Line)
		//		{
		//			var endPoint = target[pointIndex++];
		//			path.AddLineToPoint(endPoint.X, endPoint.Y);

		//		}

		//		else if (operation == PathOperation.Quad)
		//		{
		//			var controlPoint = target[pointIndex++];
		//			var endPoint = target[pointIndex++];
		//			path.AddQuadCurveToPoint(
		//				controlPoint.X,
		//				controlPoint.Y,
		//				endPoint.X,
		//				endPoint.Y);
		//		}
		//		else if (operation == PathOperation.Cubic)
		//		{
		//			var controlPoint1 = target[pointIndex++];
		//			var controlPoint2 = target[pointIndex++];
		//			var endPoint = target[pointIndex++];
		//			path.AddCurveToPoint(
		//				controlPoint1.X,
		//				controlPoint1.Y,
		//				controlPoint2.X,
		//				controlPoint2.Y,
		//				endPoint.X,
		//				endPoint.Y);
		//		}
		//		else if (operation == PathOperation.Arc)
		//		{
		//			var topLeft = target[pointIndex++];
		//			var bottomRight = target[pointIndex++];
		//			float startAngle = target.GetArcAngle(arcAngleIndex++);
		//			float endAngle = target.GetArcAngle(arcAngleIndex++);
		//			var clockwise = target.IsArcClockwise(arcClockwiseIndex++);

		//			var startAngleInRadians = GraphicsOperations.DegreesToRadians(-startAngle);
		//			var endAngleInRadians = GraphicsOperations.DegreesToRadians(-endAngle);

		//			while (startAngleInRadians < 0)
		//			{
		//				startAngleInRadians += (float)Math.PI * 2;
		//			}

		//			while (endAngleInRadians < 0)
		//			{
		//				endAngleInRadians += (float)Math.PI * 2;
		//			}

		//			var cx = (bottomRight.X + topLeft.X) / 2;
		//			var cy = (bottomRight.Y + topLeft.Y) / 2;
		//			var width = bottomRight.X - topLeft.X;
		//			var height = bottomRight.Y - topLeft.Y;
		//			var r = width / 2;

		//			var transform = CGAffineTransform.MakeTranslation(cx, cy);
		//			transform = CGAffineTransform.Multiply(CGAffineTransform.MakeScale(1, height / width), transform);

		//			path.AddArc(transform, 0, 0, r, startAngleInRadians, endAngleInRadians, !clockwise);
		//		}
		//		else if (operation == PathOperation.Close)
		//		{
		//			path.CloseSubpath();
		//		}
		//	}

		//	return path;
		//}
	}
}
