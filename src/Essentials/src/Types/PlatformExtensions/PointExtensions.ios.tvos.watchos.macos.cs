using System;
using System.Drawing;
using iOSPoint = CoreGraphics.CGPoint;

namespace Microsoft.Maui.Essentials
{
	public static class PointExtensions
	{
		public static Point ToSystemPoint(this iOSPoint point)
		{
			if (point.X > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(point.X));

			if (point.Y > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(point.Y));

			return new Point((int)point.X, (int)point.Y);
		}

		public static PointF ToSystemPointF(this iOSPoint point) =>
			new PointF((float)point.X, (float)point.Y);

		public static iOSPoint ToPlatformPoint(this Point point) =>
			new iOSPoint((nfloat)point.X, (nfloat)point.Y);

		public static iOSPoint ToPlatformPoint(this PointF point) =>
			new iOSPoint((nfloat)point.X, (nfloat)point.Y);
	}
}
