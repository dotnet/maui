using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Graphics
{
	public static class GeometryHelper
	{
		// More information: http://www.charlespetzold.com/blog/2008/01/Mathematics-of-ArcSegment.html
		public static void FlattenArc(List<Point> points, Point pt1, Point pt2, double radiusX, double radiusY, double angleRotation,
			bool isLargeArc, bool isCounterclockwise, double tolerance)
		{
			// Adjust for different radii and rotation angle
			Matrix matx = new Matrix();
			matx.Rotate(-angleRotation);
			matx.Scale(radiusY / radiusX, 1);
			pt1 = matx.Transform(pt1);
			pt2 = matx.Transform(pt2);

			// Get info about chord that connects both points
			Point midPoint = new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
			Vector2 vect = new Vector2(pt2.X - pt1.X, pt2.Y - pt1.Y);
			double halfChord = vect.Length / 2;

			// Get vector from chord to center
			Vector2 vectRotated;

			if (isLargeArc == isCounterclockwise)
				vectRotated = new Vector2(-vect.Y, vect.X);
			else
				vectRotated = new Vector2(vect.Y, -vect.X);

			vectRotated = vectRotated.Normalized;

			// Distance from chord to center
			double centerDistance = Math.Sqrt(Math.Abs((radiusY * radiusY) - (halfChord * halfChord)));

			// Calculate center point
			Point center = midPoint + centerDistance * vectRotated;

			// Get angles from center to the two points
			double angle1 = Math.Atan2(pt1.Y - center.Y, pt1.X - center.X);
			double angle2 = Math.Atan2(pt2.Y - center.Y, pt2.X - center.X);

			double sweep = Math.Abs(angle2 - angle1);
			bool reverseArc;

			if (Math.IEEERemainder(sweep + 0.000005, Math.PI) < 0.000010)
			{
				reverseArc = isCounterclockwise == angle1 < angle2;
			}
			else
			{
				bool isAcute = sweep < Math.PI;
				reverseArc = isLargeArc == isAcute;
			}

			if (reverseArc)
			{
				if (angle1 < angle2)
					angle1 += 2 * Math.PI;
				else
					angle2 += 2 * Math.PI;
			}

			// Invert matrix for final point calculation
			matx.Invert();

			// Calculate number of points for polyline approximation
			int max = (int)(4 * (radiusX + radiusY) * Math.Abs(angle2 - angle1) / (2 * Math.PI) / tolerance);

			for (int i = 0; i <= max; i++)
			{
				double angle = ((max - i) * angle1 + i * angle2) / max;
				double x = center.X + radiusY * Math.Cos(angle);
				double y = center.Y + radiusY * Math.Sin(angle);

				Point pt = matx.Transform(new Point(x, y));
				points.Add(pt);
			}
		}
	}
}