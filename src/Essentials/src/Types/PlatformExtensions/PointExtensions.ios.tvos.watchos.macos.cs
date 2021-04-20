//using System;
//using Microsoft.Maui.Graphics;
//using iOSPoint = CoreGraphics.CGPoint;

//namespace Microsoft.Maui.Essentials
//{
//	public static class PointExtensions
//	{
//		public static Point ToMauiPoint(this iOSPoint point)
//		{
//			if (point.X > int.MaxValue)
//				throw new ArgumentOutOfRangeException(nameof(point.X));

//			if (point.Y > int.MaxValue)
//				throw new ArgumentOutOfRangeException(nameof(point.Y));

//			return new Point(point.X, point.Y);
//		}

//		public static PointF ToMauiPointF(this iOSPoint point) =>
//			new PointF((float)point.X, (float)point.Y);

//		public static iOSPoint ToPlatformPoint(this Point point) =>
//			new iOSPoint((nfloat)point.X, (nfloat)point.Y);

//		public static iOSPoint ToPlatformPoint(this PointF point) =>
//			new iOSPoint((nfloat)point.X, (nfloat)point.Y);
//	}
//}
