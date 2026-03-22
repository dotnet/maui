using System;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for adding SVG-compatible arcs to a <see cref="PathF"/>.
	/// </summary>
	public static class PathArcExtensions
	{
		/// <summary>
		/// Adds an SVG arc segment to the path.
		/// </summary>
		/// <param name="aTarget">The path to which the arc segment is added.</param>
		/// <param name="rx">The x-radius of the ellipse.</param>
		/// <param name="ry">The y-radius of the ellipse.</param>
		/// <param name="angle">The rotation angle of the ellipse in degrees.</param>
		/// <param name="largeArcFlag">Determines whether the arc should be greater than or less than 180 degrees.</param>
		/// <param name="sweepFlag">Determines whether the arc should be swept in a positive or negative angle direction.</param>
		/// <param name="x">The x-coordinate of the end point of the arc.</param>
		/// <param name="y">The y-coordinate of the end point of the arc.</param>
		/// <param name="lastPointX">The x-coordinate of the start point of the arc (the current point in the path).</param>
		/// <param name="lastPointY">The y-coordinate of the start point of the arc (the current point in the path).</param>
		public static void SVGArcTo(this PathF aTarget, float rx, float ry, float angle, bool largeArcFlag, bool sweepFlag, float x, float y, float lastPointX, float lastPointY)
		{
			float[] vValues = ComputeSvgArc(rx, ry, angle, largeArcFlag, sweepFlag, x, y, lastPointX, lastPointY);
			aTarget.DrawArc(vValues[0], vValues[1], vValues[2], vValues[3], vValues[4], vValues[5], vValues[6]);
		}

		/// <summary>
		/// Converts an SVG arc specification to a Degrafa arc.
		/// </summary>
		/// <param name="rx">The x-radius of the ellipse.</param>
		/// <param name="ry">The y-radius of the ellipse.</param>
		/// <param name="angle">The rotation angle of the ellipse in degrees.</param>
		/// <param name="largeArcFlag">Determines whether the arc should be greater than or less than 180 degrees.</param>
		/// <param name="sweepFlag">Determines whether the arc should be swept in a positive or negative angle direction.</param>
		/// <param name="x">The x-coordinate of the end point of the arc.</param>
		/// <param name="y">The y-coordinate of the end point of the arc.</param>
		/// <param name="lastPointX">The x-coordinate of the start point of the arc (the current point in the path).</param>
		/// <param name="lastPointY">The y-coordinate of the start point of the arc (the current point in the path).</param>
		/// <returns>An array containing the computed arc parameters: [centerX, centerY, startAngle, sweepAngle, radiusX, radiusY, rotationAngle].</returns>
		static float[] ComputeSvgArc(float rx, float ry, float angle, bool largeArcFlag, bool sweepFlag, float x, float y, float lastPointX, float lastPointY)
		{
			//store before we do anything with it
			float xAxisRotation = angle;

			// Compute the half distance between the current and the final point
			float dx2 = (lastPointX - x) / 2.0f;
			float dy2 = (lastPointY - y) / 2.0f;

			// Convert angle from degrees to radians
			angle = GeometryUtil.DegreesToRadians(angle);
			float cosAngle = MathF.Cos(angle);
			float sinAngle = MathF.Sin(angle);

			//Compute (x1, y1)
			float x1 = cosAngle * dx2 + sinAngle * dy2;
			float y1 = -sinAngle * dx2 + cosAngle * dy2;

			// Ensure radii are large enough
			rx = MathF.Abs(rx);
			ry = MathF.Abs(ry);
			float prx = rx * rx;
			float pry = ry * ry;
			float px1 = x1 * x1;
			float py1 = y1 * y1;

			// check that radii are large enough
			float radiiCheck = px1 / prx + py1 / pry;
			if (radiiCheck > 1)
			{
				rx = MathF.Sqrt(radiiCheck) * rx;
				ry = MathF.Sqrt(radiiCheck) * ry;
				prx = rx * rx;
				pry = ry * ry;
			}

			//Compute (cx1, cy1)
			float sign = largeArcFlag == sweepFlag ? -1 : 1;
			float sq = (prx * pry - prx * py1 - pry * px1) / (prx * py1 + pry * px1);
			sq = sq < 0 ? 0 : sq;
			float coef = sign * MathF.Sqrt(sq);
			float cx1 = coef * (rx * y1 / ry);
			float cy1 = coef * -(ry * x1 / rx);

			//Compute (cx, cy) from (cx1, cy1)
			float sx2 = (lastPointX + x) / 2.0f;
			float sy2 = (lastPointY + y) / 2.0f;
			float cx = sx2 + (cosAngle * cx1 - sinAngle * cy1);
			float cy = sy2 + (sinAngle * cx1 + cosAngle * cy1);

			//Compute the angleStart (angle1) and the angleExtent (dangle)
			float ux = (x1 - cx1) / rx;
			float uy = (y1 - cy1) / ry;
			float vx = (-x1 - cx1) / rx;
			float vy = (-y1 - cy1) / ry;

			//Compute the angle start
			float n = MathF.Sqrt(ux * ux + uy * uy);
			float p = ux;

			sign = uy < 0 ? -1.0f : 1.0f;

			float angleStart = GeometryUtil.RadiansToDegrees(sign * MathF.Acos(p / n));

			// Compute the angle extent
			n = MathF.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
			p = ux * vx + uy * vy;
			sign = ux * vy - uy * vx < 0 ? -1.0f : 1.0f;
			float angleExtent = GeometryUtil.RadiansToDegrees(sign * MathF.Acos(p / n));

			if (!sweepFlag && angleExtent > 0)
			{
				angleExtent -= 360;
			}
			else if (sweepFlag && angleExtent < 0)
			{
				angleExtent += 360;
			}

			angleExtent %= 360;
			angleStart %= 360;

			return new[] { cx, cy, angleStart, angleExtent, rx, ry, xAxisRotation };
		}

		/**
		* Draws an arc of type "open" only. Accepts an optional x axis rotation value
		**/

		public static void DrawArc(this PathF aPath, float x, float y, float startAngle, float arc, float radius, float yRadius, float xAxisRotation)
		{
			// Circumvent drawing more than is needed
			if (MathF.Abs(arc) > 360)
			{
				arc = 360;
			}

			// Draw in a maximum of 45 degree segments. First we calculate how many
			// segments are needed for our arc.
			float segs = MathF.Ceiling(MathF.Abs(arc) / 45);

			// Now calculate the sweep of each segment
			float segAngle = arc / segs;

			float theta = GeometryUtil.DegreesToRadians(segAngle);
			float angle = GeometryUtil.DegreesToRadians(startAngle);

			// Draw as 45 degree segments
			if (segs > 0)
			{
				float beta = GeometryUtil.DegreesToRadians(xAxisRotation);
				float sinbeta = MathF.Sin(beta);
				float cosbeta = MathF.Cos(beta);

				// Loop for drawing arc segments
				for (int i = 0; i < segs; i++)
				{
					angle += theta;

					float sinangle = MathF.Sin(angle - theta / 2);
					float cosangle = MathF.Cos(angle - theta / 2);

					float div = MathF.Cos(theta / 2);
					float cx = x + (radius * cosangle * cosbeta - yRadius * sinangle * sinbeta) / div;
					//Why divide by Math.cos(theta/2)? - FIX THIS
					float cy = y + (radius * cosangle * sinbeta + yRadius * sinangle * cosbeta) / div;
					//Why divide by Math.cos(theta/2)? - FIX THIS

					sinangle = MathF.Sin(angle);
					cosangle = MathF.Cos(angle);

					float x1 = x + (radius * cosangle * cosbeta - yRadius * sinangle * sinbeta);
					float y1 = y + (radius * cosangle * sinbeta + yRadius * sinangle * cosbeta);

					aPath.QuadTo(cx, cy, x1, y1);
				}
			}
		}
	}
}
