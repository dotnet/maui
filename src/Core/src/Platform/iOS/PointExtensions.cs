using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Maui
{
	public static class PointExtensions
	{
		public static Point ToPoint(this PointF point)
		{
			return new Point(point.X, point.Y);
		}

		public static PointF ToNative(this Point point)
		{
			return new PointF(point.X, point.Y);
		}
	}
}
