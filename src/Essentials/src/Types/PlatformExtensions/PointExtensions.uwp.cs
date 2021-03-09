using System;
using System.Drawing;
using WindowsPoint = Windows.Foundation.Point;

namespace Microsoft.Maui.Essentials
{
    public static class PointExtensions
    {
        public static Point ToSystemPoint(this WindowsPoint point)
        {
            if (point.X > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(point.X));

            if (point.Y > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(point.Y));

            return new Point((int)point.X, (int)point.Y);
        }

        public static PointF ToSystemPointF(this WindowsPoint point) =>
            new PointF((float)point.X, (float)point.Y);

        public static WindowsPoint ToPlatformPoint(this Point point) =>
            new WindowsPoint(point.X, point.Y);

        public static WindowsPoint ToPlatformPoint(this PointF point) =>
            new WindowsPoint(point.X, point.Y);
    }
}
