using System.Drawing;
using AndroidPoint = Android.Graphics.Point;
using AndroidPointF = Android.Graphics.PointF;

namespace Microsoft.Maui.Essentials
{
	public static class PointExtensions
	{
		public static Point ToSystemPoint(this AndroidPoint point) =>
			new Point(point.X, point.Y);

		public static PointF ToSystemPointF(this AndroidPointF point) =>
			new PointF(point.X, point.Y);

		public static AndroidPoint ToPlatformPoint(this Point point) =>
			new AndroidPoint(point.X, point.Y);

		public static AndroidPointF ToPlatformPointF(this PointF point) =>
			new AndroidPointF(point.X, point.Y);
	}
}
