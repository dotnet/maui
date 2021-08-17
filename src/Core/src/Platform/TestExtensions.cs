#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class TestExtensions
    {
        public static bool IsEmpty(this CornerRadius? cornerRadius)
        {
            return cornerRadius == null;
        }

        public static bool IsAllRadius(this CornerRadius cornerRadius)
        {
            return Math.Abs((
                (cornerRadius.TopLeft +
                cornerRadius.TopRight +
                cornerRadius.BottomLeft +
                cornerRadius.BottomRight) / 4) - cornerRadius.TopLeft) < 0.001;
        }

        public static float[]? ToStartEndPoint(this float angle) => ((double)angle).ToStartEndPoint();

        public static float[]? ToStartEndPoint(this double angle)
        {
            var points = angle.ToPoints().ToArray();

            if (points.Length != 2)
                return null;

            return new[]
            {
                (float)points[0].X,
                (float)points[0].Y,
                (float)points[1].X,
                (float)points[1].Y
            };
        }
        public static IEnumerable<Point> ToPoints(this float angle) => ((double)angle).ToPoints();

        public static IEnumerable<Point> ToPoints(this double angle)
        {
            var d = Math.Pow(2, .5);
            var eps = Math.Pow(2, -52);

            var finalAngle = angle % 360;

            var startPointRadians = (180 - finalAngle).ToRadians();
            var startX = d * Math.Cos(startPointRadians);
            var startY = d * Math.Sin(startPointRadians);

            var endPointRadians = (360 - finalAngle).ToRadians();
            var endX = d * Math.Cos(endPointRadians);
            var endY = d * Math.Sin(endPointRadians);

            return new[]
            {
                new Point(startX.CheckForOverflow(eps), startY.CheckForOverflow(eps)),
                new Point(endX.CheckForOverflow(eps), endY.CheckForOverflow(eps))
            };
        }

        public static double ToRadians(this int angle)
        {
            return Math.PI * angle / 180;
        }

        public static double ToRadians(this float angle)
        {
            return Math.PI * angle / 180;
        }

        public static double ToRadians(this double angle)
        {
            return Math.PI * angle / 180;
        }

        private static double CheckForOverflow(this double value, double eps)
        {
            return value <= 0 || Math.Abs(value) <= eps ? 0f : value;
        }

        public static float[] ToRadii(this CornerRadius cornerRadius, double density)
        {
            return new[]
            {
                ToPixels(cornerRadius.TopLeft, density),
                ToPixels(cornerRadius.TopLeft, density),
                ToPixels(cornerRadius.TopRight, density),
                ToPixels(cornerRadius.TopRight, density),
                ToPixels(cornerRadius.BottomRight, density),
                ToPixels(cornerRadius.BottomRight, density),
                ToPixels(cornerRadius.BottomLeft, density),
                ToPixels(cornerRadius.BottomLeft, density)
            };
        }

        public static float ToPixels(double units, double density)
        {
            return (float)(units * density);
        }
    }
}