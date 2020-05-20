using System.Maui.Graphics;
using Android.Graphics;
using APath = Android.Graphics.Path;
using Path = System.Maui.Graphics.Path;

namespace System.Maui
{
	public static class GraphicsExtensions
	{
		public static APath AsAndroidPath(this Path path)
		{
			var nativePath = new APath();

			int pointIndex = 0;
			int arcAngleIndex = 0;
			int arcClockwiseIndex = 0;

			foreach (var operation in path.PathOperations)
			{
				if (operation == PathOperation.MoveTo)
				{
					var point = path[pointIndex++];
					nativePath.MoveTo((float)point.X, (float)point.Y);
				}
				else if (operation == PathOperation.Line)
				{
					var point = path[pointIndex++];
					nativePath.LineTo((float)point.X, (float)point.Y);
				}

				else if (operation == PathOperation.Quad)
				{
					var controlPoint = path[pointIndex++];
					var point = path[pointIndex++];
					nativePath.QuadTo((float)controlPoint.X, (float)controlPoint.Y, (float)point.X, (float)point.Y);
				}
				else if (operation == PathOperation.Cubic)
				{
					var controlPoint1 = path[pointIndex++];
					var controlPoint2 = path[pointIndex++];
					var point = path[pointIndex++];
					nativePath.CubicTo((float)controlPoint1.X, (float)controlPoint1.Y, (float)controlPoint2.X, (float)controlPoint2.Y, (float)point.X,
						(float)point.Y);
				}
				else if (operation == PathOperation.Arc)
				{
					var topLeft = path[pointIndex++];
					var bottomRight = path[pointIndex++];
					var startAngle = path.GetArcAngle(arcAngleIndex++);
					var endAngle = path.GetArcAngle(arcAngleIndex++);
					var clockwise = path.IsArcClockwise(arcClockwiseIndex++);

					while (startAngle < 0)
					{
						startAngle += 360;
					}

					while (endAngle < 0)
					{
						endAngle += 360;
					}

					var rect = new RectF((float)topLeft.X, (float)topLeft.Y, (float)bottomRight.X, (float)bottomRight.Y);
					var sweep = GraphicsOperations.GetSweep(startAngle, endAngle, clockwise);

					startAngle *= -1;
					if (!clockwise)
					{
						sweep *= -1;
					}

					nativePath.ArcTo(rect, (float)startAngle, (float)sweep);
				}
				else if (operation == PathOperation.Close)
				{
					nativePath.Close();
				}
				else
				{
					System.Console.WriteLine("hmm");
				}
			}

			return nativePath;
		}

		public static Rect ToRect(this System.Maui.Rectangle r) => new Rect((int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom);
	}
}
