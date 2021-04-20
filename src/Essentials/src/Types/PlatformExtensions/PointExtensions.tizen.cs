//using Microsoft.Maui.Graphics;
//using EPoint = ElmSharp.Point;

//namespace Microsoft.Maui.Essentials
//{
//	public static class PointExtensions
//	{
//		public static Point ToMauiPoint(this EPoint point) =>
//			new Point(point.X, point.Y);

//		public static PointF ToMauiPointF(this EPoint point) =>
//			new PointF(point.X, point.Y);

//		public static EPoint ToPlatformPoint(this Point point) =>
//			new EPoint() { X = point.X, Y = point.Y };

//		public static EPoint ToPlatformPoint(this PointF point) =>
//			ToPlatformPointF(point);

//		public static EPoint ToPlatformPointF(this PointF point) =>
//			new EPoint() { X = (int)point.X, Y = (int)point.Y };
//	}
//}
