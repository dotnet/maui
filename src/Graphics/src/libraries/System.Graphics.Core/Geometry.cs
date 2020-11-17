namespace System.Graphics
{
    public static class Geometry
    {
        public const float Epsilon = 0.0000000001f;

        public static float GetDistance(float x1, float y1, float x2, float y2)
        {
            var a = x2 - x1;
            var b = y2 - y1;

            return (float) Math.Sqrt(a * a + b * b);
        }



        public static float GetAngleAsDegrees(float x1, float y1, float x2, float y2)
        {
            try
            {
                var dx = x1 - x2;
                var dy = y1 - y2;

                var radians = (float) Math.Atan2(dy, dx);
                var degrees = radians * 180.0f / (float) Math.PI;

                return 180 - degrees;
            }
            catch (Exception exc)
            {
                Logger.Warn(exc);
                throw new Exception("Exception in GetAngleAsDegrees", exc);
            }
        }
        
        public static float DegreesToRadians(float angle)
        {
            return (float) Math.PI * angle / 180;
        }

        public static double DegreesToRadians(double angle)
        {
            return Math.PI * angle / 180;
        }

        public static float RadiansToDegrees(float angle)
        {
            return angle * (180 / (float) Math.PI);
        }

        public static double RadiansToDegrees(double angle)
        {
            return angle * (180 / Math.PI);
        }

        public static PointF RotatePoint(PointF point, float angle)
        {
            var radians = DegreesToRadians(angle);

            var x = (float) (Math.Cos(radians) * point.X - Math.Sin(radians) * point.Y);
            var y = (float) (Math.Sin(radians) * point.X + Math.Cos(radians) * point.Y);

            return new PointF(x, y);
        }

        public static PointF RotatePoint(PointF center, PointF point, float angle)
        {
            var radians = DegreesToRadians(angle);
            var x = center.X + (float) (Math.Cos(radians) * (point.X - center.X) - Math.Sin(radians) * (point.Y - center.Y));
            var y = center.Y + (float) (Math.Sin(radians) * (point.X - center.X) + Math.Cos(radians) * (point.Y - center.Y));
            return new PointF(x, y);
        }

        public static float GetSweep(float angle1, float angle2, bool clockwise)
        {
            if (clockwise)
            {
                if (angle2 > angle1)
                {
                    return angle1 + (360 - angle2);
                }
                else
                {
                    return angle1 - angle2;
                }
            }
            else
            {
                if (angle1 > angle2)
                {
                    return angle2 + (360 - angle1);
                }
                else
                {
                    return angle2 - angle1;
                }
            }
        }
        
        public static PointF PolarToPoint(float angleInRadians, float fx, float fy)
        {
            var sin = (float) Math.Sin(angleInRadians);
            var cos = (float) Math.Cos(angleInRadians);
            return new PointF(fx * cos, fy * sin);
        }


        /// <summary>
        /// Gets the point on an oval that corresponds to the given angle.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="x">The x position of the bounding rectangle.</param>
        /// <param name="y">The y position of the bounding rectangle.</param>
        /// <param name="width">The width of the bounding rectangle.</param>
        /// <param name="height">The height of the bounding rectangle.</param>
        /// <param name="angleInDegrees">Angle in degrees.</param>
        public static PointF OvalAngleToPoint(float x, float y, float width, float height, float angleInDegrees)
        {
            var radians = DegreesToRadians(angleInDegrees);

            var cx = x + width / 2;
            var cy = y + height / 2;

            var point = PolarToPoint(radians, width / 2, height / 2);

            point.X += cx;
            point.Y += cy;
            return point;
        }
        
        public static PointF GetOppositePoint(PointF pivot, PointF oppositePoint)
        {
            var dx = oppositePoint.X - pivot.X;
            var dy = oppositePoint.Y - pivot.Y;
            return new PointF(pivot.X - dx, pivot.Y - dy);
        }
        
        /**
       * Return true if c is between a and b.
        */

        private static bool IsBetween(float a, float b, float c)
        {
            return b > a ? c >= a && c <= b : c >= b && c <= a;
        }

        /**
         * Check if two points are on the same side of a given line.
         * Algorithm from Sedgewick page 350.
         * 
         * @param x0, y0, x1, y1  The line.
         * @param px0, py0        First point.
         * @param px1, py1        Second point.
         * @return                <0 if points on opposite sides.
         *                        =0 if one of the points is exactly on the line
         *                        >0 if points on same side.
         */

        private static int SameSide(float x0, float y0, float x1, float y1, float px0, float py0, float px1, float py1)
        {
            var sameSide = 0;

            var dx = x1 - x0;
            var dy = y1 - y0;
            var dx1 = px0 - x0;
            var dy1 = py0 - y0;
            var dx2 = px1 - x1;
            var dy2 = py1 - y1;

            // Cross product of the vector from the endpoint of the line to the point
            var c1 = dx * dy1 - dy * dx1;
            var c2 = dx * dy2 - dy * dx2;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (c1 != 0 && c2 != 0)
            {
                sameSide = c1 < 0 != c2 < 0 ? -1 : 1;
            }
            else if (dx == 0 && dx1 == 0 && dx2 == 0)
            {
                sameSide = !IsBetween(y0, y1, py0) && !IsBetween(y0, y1, py1) ? 1 : 0;
            }
            else if (dy == 0 && dy1 == 0 && dy2 == 0)
            {
                sameSide = !IsBetween(x0, x1, px0) && !IsBetween(x0, x1, px1) ? 1 : 0;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            return sameSide;
        }

        /**
         * Check if two line segments intersects. Integer domain.
         * 
         * @param x0, y0, x1, y1  End points of first line to check.
         * @param x2, yy, x3, y3  End points of second line to check.
         * @return                True if the two lines intersects.
         */

        public static bool IsLineIntersectingLine(
            float x0,
            float y0,
            float x1,
            float y1,
            float x2,
            float y2,
            float x3,
            float y3)
        {
            var s1 = SameSide(x0, y0, x1, y1, x2, y2, x3, y3);
            var s2 = SameSide(x2, y2, x3, y3, x0, y0, x1, y1);

            return s1 <= 0 && s2 <= 0;
        }

        
        public static float GetFactor(float aMin, float aMax, float aValue)
        {
            var vAdjustedValue = aValue - aMin;
            var vRange = aMax - aMin;

            if (Math.Abs(vAdjustedValue - vRange) < Epsilon)
            {
                return 1;
            }

            return vAdjustedValue / vRange;
        }

        public static float GetLinearValue(float aMin, float aMax, float aFactor)
        {
            var d = aMax - aMin;
            d *= aFactor;
            return aMin + d;
        }
    }
}