using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a point in 2D space using double-precision floating-point coordinates.
	/// </summary>
	[DebuggerDisplay("X={X}, Y={Y}")]
	[TypeConverter(typeof(Converters.PointTypeConverter))]
	public partial struct Point
	{
		/// <summary>
		/// Gets or sets the X-coordinate of this point.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Gets or sets the Y-coordinate of this point.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Represents a <see cref="Point"/> with coordinates (0,0).
		/// </summary>
		public static Point Zero = new Point();

		/// <summary>
		/// Returns a string representation of this <see cref="Point"/>.
		/// </summary>
		/// <returns>A string in the format "X=0 Y=0" (values may vary).</returns>
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

		public Point(Vector2 v)
		{
			X = v.X;
			Y = v.Y;
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

			var compareTo = (Point)o;
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

		public static Size operator -(Point ptA, Point ptB)
		{
			return new Size(ptA.X - ptB.X, ptA.Y - ptB.Y);
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

		public static implicit operator Point(Vector2 v) => new Point(v);

		public static bool TryParse(string value, out Point point)
		{
			// IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
			if (!string.IsNullOrEmpty(value))
			{
				string[] xy = value.Split(',');
				if (xy.Length == 2 && double.TryParse(xy[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var x)
					&& double.TryParse(xy[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var y))
				{
					point = new Point(x, y);
					return true;
				}
			}

			point = default;
			return false;
		}
	}
}
