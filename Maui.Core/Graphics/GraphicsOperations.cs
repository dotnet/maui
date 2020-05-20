using System;
using System.Collections.Generic;
using System.Linq;
using System.Maui;

namespace System.Maui.Graphics {
	public static class GraphicsOperations
	{
		public static readonly double Epsilon = double.Epsilon;

		public static Point RotatePoint(Point point, double angleInDegrees)
		{
			var radians = DegreesToRadians(angleInDegrees);

			var x = (Math.Cos(radians) * point.X - Math.Sin(radians) * point.Y);
			var y = (Math.Sin(radians) * point.X + Math.Cos(radians) * point.Y);

			return new Point(x, y);
		}

		public static Point RotatePoint(Point center, Point point, double angleInDegrees)
		{
			var radians = DegreesToRadians(angleInDegrees);
			var x = center.X + (double)(Math.Cos(radians) * (point.X - center.X) - Math.Sin(radians) * (point.Y - center.Y));
			var y = center.Y + (double)(Math.Sin(radians) * (point.X - center.X) + Math.Cos(radians) * (point.Y - center.Y));
			return new Point(x, y);
		}

		public static float DegreesToRadians(float angleInDegrees)
		{
			return (float)Math.PI * angleInDegrees / 180;
		}

		public static double DegreesToRadians(double angleInDegrees)
		{
			return Math.PI * angleInDegrees / 180;
		}

		public static float RadiansToDegrees (float angleInRadians)
		{
			return angleInRadians * (180 / (float)Math.PI);
		}

		public static double RadiansToDegrees(double angleInRadians)
		{
			return angleInRadians * (180 / Math.PI);
		}

		public static double GetSweep(double angle1, double angle2, bool clockwise)
		{
			if (clockwise)
			{
				if (angle2 > angle1)
					return angle1 + (360 - angle2);

				return angle1 - angle2;
			}

			if (angle1 > angle2)
				return angle2 + (360 - angle1);

			return angle2 - angle1;
		}

		public static Rectangle GetBoundsOfQuadraticCurve(
			Point startPoint,
			Point controlPoint,
			Point endPoint)
		{
			return GetBoundsOfQuadraticCurve(startPoint.X, startPoint.Y, controlPoint.X, controlPoint.Y, endPoint.X, endPoint.Y);
		}

		public static Rectangle GetBoundsOfQuadraticCurve(
			double x0, double y0,
			double x1, double y1,
			double x2, double y2)
		{
			var cpx0 = x0 + 2.0f * (x1 - x0) / 3.0f;
			var cpy0 = y0 + 2.0f * (y1 - y0) / 3.0f;
			var cpx1 = x2 + 2.0f * (x1 - x1) / 3.0f;
			var cpy1 = y2 + 2.0f * (y1 - y1) / 3.0f;

			return GetBoundsOfCubicCurve(
				x0, y1,
				cpx0, cpy0,
				cpx1, cpy1,
				x2, y2);
		}

		public static Rectangle GetBoundsOfCubicCurve(
			Point startPoint,
			Point controlPoint1,
			Point controlPoint2,
			Point endPoint)
		{
			return GetBoundsOfCubicCurve(startPoint.X, startPoint.Y, controlPoint1.X, controlPoint1.Y, controlPoint2.X, controlPoint2.Y, endPoint.X, endPoint.Y);
		}


		public static Rectangle GetBoundsOfCubicCurve(
			double x0, double y0,
			double x1, double y1,
			double x2, double y2,
			double x3, double y3)
		{
			var tValues = new List<double>();

			double t;

			for (var i = 0; i < 2; ++i)
			{
				double b;
				double c;
				double a;

				if (i == 0)
				{
					b = 6 * x0 - 12 * x1 + 6 * x2;
					a = -3 * x0 + 9 * x1 - 9 * x2 + 3 * x3;
					c = 3 * x1 - 3 * x0;
				}
				else
				{
					b = 6 * y0 - 12 * y1 + 6 * y2;
					a = -3 * y0 + 9 * y1 - 9 * y2 + 3 * y3;
					c = 3 * y1 - 3 * y0;
				}

				if (Math.Abs(a) < Epsilon)
				{
					if (Math.Abs(b) < Epsilon)
					{
						continue;
					}

					t = -c / b;
					if (0 < t && t < 1)
					{
						tValues.Add(t);
					}

					continue;
				}

				var b2ac = b * b - 4 * c * a;
				var sqrtb2ac = (double)Math.Sqrt(b2ac);
				if (b2ac < 0)
				{
					continue;
				}

				var t1 = (-b + sqrtb2ac) / (2 * a);
				if (0 < t1 && t1 < 1)
				{
					tValues.Add(t1);
				}

				var t2 = (-b - sqrtb2ac) / (2 * a);
				if (0 < t2 && t2 < 1)
				{
					tValues.Add(t2);
				}
			}

			var xValues = new List<double>();
			var yValues = new List<double>();

			for (var j = tValues.Count - 1; j >= 0; j--)
			{
				t = tValues[j];
				var mt = 1 - t;
				var x = (mt * mt * mt * x0) + (3 * mt * mt * t * x1) + (3 * mt * t * t * x2) + (t * t * t * x3);
				var y = (mt * mt * mt * y0) + (3 * mt * mt * t * y1) + (3 * mt * t * t * y2) + (t * t * t * y3);

				xValues.Add(x);
				yValues.Add(y);
			}

			xValues.Add(x0);
			xValues.Add(x3);
			yValues.Add(y0);
			yValues.Add(y3);

			var minX = xValues.Min();
			var minY = yValues.Min();
			var maxX = xValues.Max();
			var maxY = yValues.Max();

			return new Rectangle(minX, minY, maxX - minX, maxY - minY);
		}

		public static Point GetPointAtAngle(double x, double y, double distance, double radians)
		{
			var x2 = x + (Math.Cos(radians) * distance);
			var y2 = y + (Math.Sin(radians) * distance);
			return new Point((double)x2, (double)y2);
		}

		public static Point GetPointOnOval(
			double x,
			double y,
			double width,
			double height,
			double angle)
		{
			var cx = x + (width / 2);
			var cy = y + (height / 2);

			var d = Math.Max(width, height);
			var d2 = d / 2;
			var fx = width / d;
			var fy = height / d;

			while (angle >= 360)
				angle -= 360;

			angle *= -1;

			var radians = (double)DegreesToRadians(angle);
			var point = GetPointAtAngle(0, 0, d2, radians);
			point.X = cx + (point.X * fx);
			point.Y = cy + (point.Y * fy);

			return point;
		}

		public static double GetAngleAsDegrees(Point point1, Point point2)
		{
			var dx = point1.X - point2.X;
			var dy = point1.Y - point2.Y;

			var radians = (double)Math.Atan2(dy, dx);
			var degrees = radians * 180.0f / (double)Math.PI;

			return 180 - degrees;
		}

		public static Rectangle GetBoundsOfArc(
			double x,
			double y,
			double width,
			double height,
			double angle1,
			double angle2,
			bool clockwise)
		{
			var x1 = x;
			var y1 = y;
			var x2 = x + width;
			var y2 = y + height;

			var point1 = GetPointOnOval(x, y, width, height, angle1);
			var point2 = GetPointOnOval(x, y, width, height, angle2);
			var center = new Point(x + width / 2, y + height / 2);

			var startAngle = GetAngleAsDegrees(center, point1);
			var endAngle = GetAngleAsDegrees(center, point2);

			var startAngleRadians = DegreesToRadians(startAngle);
			var endAngleRadians = DegreesToRadians(endAngle);

			var quadrant1 = GetQuadrant(startAngleRadians);
			var quadrant2 = GetQuadrant(endAngleRadians);

			if (quadrant1 == quadrant2)
			{
				if (clockwise)
				{
					if (((quadrant1 == 1 || quadrant1 == 2) && (point1.X < point2.X)) || ((quadrant1 == 3 || quadrant1 == 4) && (point1.X > point2.X)))
					{
						x1 = Math.Min(point1.X, point2.X);
						y1 = Math.Min(point1.Y, point2.Y);
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
				}
				else
				{
					if (((quadrant1 == 1 || quadrant1 == 2) && (point1.X > point2.X)) || ((quadrant1 == 3 || quadrant1 == 4) && (point1.X < point2.X)))
					{
						x1 = Math.Min(point1.X, point2.X);
						y1 = Math.Min(point1.Y, point2.Y);
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
				}
			}
			else if (quadrant1 == 1)
			{
				if (clockwise)
				{
					if (quadrant2 == 4)
					{
						x1 = Math.Min(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
						y1 = Math.Min(point1.Y, point2.Y);
					}
					else if (quadrant2 == 3)
					{
						y1 = Math.Min(point1.Y, point2.Y);
						x1 = Math.Min(point1.X, point2.X);
					}
					else if (quadrant2 == 2)
					{
						y1 = Math.Min(point1.Y, point2.Y);
					}
				}
				else
				{
					if (quadrant2 == 2)
					{
						x1 = Math.Min(point1.X, point2.X);
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 3)
					{
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 4)
					{
						x2 = Math.Max(point1.X, point2.X);
					}
				}
			}
			else if (quadrant1 == 2)
			{
				if (clockwise)
				{
					if (quadrant2 == 1)
					{
						x1 = Math.Min(point1.X, point2.X);
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 4)
					{
						x1 = Math.Min(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 3)
					{
						x1 = Math.Min(point1.X, point2.X);
					}
				}
				else
				{
					if (quadrant2 == 3)
					{
						y1 = Math.Min(point1.Y, point2.Y);
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 4)
					{
						y1 = Math.Min(point1.Y, point2.Y);
						x2 = Math.Max(point1.X, point2.X);
					}
					else if (quadrant2 == 1)
					{
						y1 = Math.Min(point1.Y, point2.Y);
					}
				}
			}
			else if (quadrant1 == 3)
			{
				if (clockwise)
				{
					if (quadrant2 == 2)
					{
						y1 = Math.Min(point1.Y, point2.Y);
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 1)
					{
						x2 = Math.Max(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 4)
					{
						y2 = Math.Max(point1.Y, point2.Y);
					}
				}
				else
				{
					if (quadrant2 == 4)
					{
						x1 = Math.Min(point1.X, point2.X);
						x2 = Math.Max(point1.X, point2.X);
						y1 = Math.Min(point1.Y, point2.Y);
					}
					else if (quadrant2 == 1)
					{
						x1 = Math.Min(point1.X, point2.X);
						y1 = Math.Min(point1.Y, point2.Y);
					}
					else if (quadrant2 == 2)
					{
						x1 = Math.Min(point1.X, point2.X);
					}
				}
			}
			else if (quadrant1 == 4)
			{
				if (clockwise)
				{
					if (quadrant2 == 3)
					{
						x1 = Math.Min(point1.X, point2.X);
						x2 = Math.Max(point1.X, point2.X);
						y1 = Math.Min(point1.Y, point2.Y);
					}
					else if (quadrant2 == 2)
					{
						x2 = Math.Max(point1.X, point2.X);
						y1 = Math.Min(point1.Y, point2.Y);
					}
					else if (quadrant2 == 1)
					{
						x2 = Math.Max(point1.X, point2.X);
					}
				}
				else
				{
					if (quadrant2 == 1)
					{
						x1 = Math.Min(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
						y1 = Math.Min(point1.Y, point2.Y);
					}
					else if (quadrant2 == 2)
					{
						x1 = Math.Min(point1.X, point2.X);
						y2 = Math.Max(point1.Y, point2.Y);
					}
					else if (quadrant2 == 3)
					{
						y2 = Math.Max(point1.Y, point2.Y);
					}
				}
			}

			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}

		public static byte GetQuadrant(double radians)
		{
			var trueAngle = radians % (2 * Math.PI);
			if (trueAngle >= 0.0 && trueAngle < Math.PI / 2.0)
				return 1;
			if (trueAngle >= Math.PI / 2.0 && trueAngle < Math.PI)
				return 2;
			if (trueAngle >= Math.PI && trueAngle < Math.PI * 3.0 / 2.0)
				return 3;
			if (trueAngle >= Math.PI * 3.0 / 2.0 && trueAngle < Math.PI * 2)
				return 4;
			return 0;
		}

		public static Point GetOppositePoint(Point pivot, Point oppositePoint)
		{
			var dx = oppositePoint.X - pivot.X;
			var dy = oppositePoint.Y - pivot.Y;
			return new Point(pivot.X - dx, pivot.Y - dy);
		}

		public static Point PolarToPoint(double aAngleInRadians, double fx, double fy)
		{
			var sin = (double)Math.Sin(aAngleInRadians);
			var cos = (double)Math.Cos(aAngleInRadians);
			return new Point(fx * cos, fy * sin);
		}

		public static Point OvalAngleToPoint(double x, double y, double width, double height, double aAngleInDegrees)
		{
			double vAngle = DegreesToRadians(aAngleInDegrees);

			double cx = x + width / 2;
			double cy = y + height / 2;

			Point vPoint = PolarToPoint(vAngle, width / 2, height / 2);

			vPoint.X += cx;
			vPoint.Y += cy;
			return vPoint;
		}
	}
}
