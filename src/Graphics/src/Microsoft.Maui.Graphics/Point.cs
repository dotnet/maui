using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	[DebuggerDisplay("X={X}, Y={Y}")]
	public partial struct Point
	{
		public double X { get; set; }

		public double Y { get; set; }

		public static Point Zero = new Point();

		public override string ToString()
		{
			return $"{{X={X.ToString(CultureInfo.InvariantCulture)} Y={Y.ToString(CultureInfo.InvariantCulture)}}}";
		}

		public Point(double x, double y) : this()
		{
			X = x;
			Y = y;
		}

		public Point(Size sz) : this()
		{
			X = sz.Width;
			Y = sz.Height;
		}

		public Point(SizeF sz) : this()
		{
			X = sz.Width;
			Y = sz.Height;
		}

		public override bool Equals(object o)
		{
			if (!(o is Point))
				return false;

			return this == (Point)o;
		}

		public bool Equals(object o, double epsilon)
		{
			if (!(o is Point))
				return false;

			var compareTo = (Point) o;
			return Math.Abs(compareTo.X - X) < epsilon && Math.Abs(compareTo.Y - Y) < epsilon;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() * 397);
		}

		public Point Offset(double dx, double dy)
		{
			Point p = this;
			p.X += dx;
			p.Y += dy;
			return p;
		}

		public Point Round()
		{
			return new Point((double)Math.Round(X), (double)Math.Round(Y));
		}

		public bool IsEmpty => X == 0 && Y == 0;

		public static explicit operator Size(Point pt)
		{
			return new Size(pt.X, pt.Y);
		}

		public static Point operator +(Point pt, SizeF sz)
		{
			return new Point(pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static Point operator -(Point pt, SizeF sz)
		{
			return new Point(pt.X - sz.Width, pt.Y - sz.Height);
		}

		public static bool operator ==(Point ptA, Point ptB)
		{
			return ptA.X == ptB.X && ptA.Y == ptB.Y;
		}

		public static bool operator !=(Point ptA, Point ptB)
		{
			return ptA.X != ptB.X || ptA.Y != ptB.Y;
		}

		public double Distance(Point other)
		{
			return (double)Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
		}

		public void Deconstruct(out double x, out double y)
		{
			x = X;
			y = Y;
		}

		public static implicit operator PointF(Point p) => new PointF((float)p.X, (float)p.Y);
	}
}
