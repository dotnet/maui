using System;
using System.Diagnostics;
using System.Globalization;

namespace Xamarin.Forms
{
	[DebuggerDisplay("X={X}, Y={Y}, Width={Width}, Height={Height}")]
	[TypeConverter(typeof(RectTypeConverter))]
	public struct Rect
	{
		public Rect(double x, double y, double width, double height) : this()
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public Rect(Point loc, Size sz) : this(loc.X, loc.Y, sz.Width, sz.Height)
		{
		}

		public double X { get; set; }

		public double Y { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		public static Rect Zero = new Rect();

		public double Top => Y;

		public double Bottom => Y + Height;

		public double Right => X + Width;

		public double Left => X;

		public bool IsEmpty => (Width <= 0) || (Height <= 0);

		public Size Size
		{
			get => new Size(Width, Height);
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}

		public Point Location
		{
			get => new Point(X, Y);
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		public Point Center => new Point(X + Width / 2, Y + Height / 2);

		public static Rect FromLTRB(double left, double top, double right, double bottom) => new Rect(left, top, right - left, bottom - top);

		public bool Equals(Rect other) => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;

			return obj is Rect rect && Equals(rect) || obj is Rectangle rectangle && Equals(rectangle);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Width.GetHashCode();
				hashCode = (hashCode * 397) ^ Height.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Rect r1, Rect r2) => (r1.Location == r2.Location) && (r1.Size == r2.Size);

		public static bool operator !=(Rect r1, Rect r2) => !(r1 == r2);

		// Hit Testing / Intersection / Union
		public bool Contains(Rect rect) => X <= rect.X && Right >= rect.Right && Y <= rect.Y && Bottom >= rect.Bottom;

		public bool Contains(Point pt) => Contains(pt.X, pt.Y);

		public bool Contains(double x, double y) => (x >= Left) && (x < Right) && (y >= Top) && (y < Bottom);

		public bool IntersectsWith(Rect r) => !((Left >= r.Right) || (Right <= r.Left) || (Top >= r.Bottom) || (Bottom <= r.Top));

		public Rect Union(Rect r) => Union(this, r);

		public static Rect Union(Rect r1, Rect r2) => FromLTRB(Math.Min(r1.Left, r2.Left), Math.Min(r1.Top, r2.Top), Math.Max(r1.Right, r2.Right), Math.Max(r1.Bottom, r2.Bottom));

		public Rect Intersect(Rect r) => Intersect(this, r);

		public static Rect Intersect(Rect r1, Rect r2)
		{
			double x = Math.Max(r1.X, r2.X);
			double y = Math.Max(r1.Y, r2.Y);
			double width = Math.Min(r1.Right, r2.Right) - x;
			double height = Math.Min(r1.Bottom, r2.Bottom) - y;

			if (width < 0 || height < 0)
				return Zero;

			return new Rect(x, y, width, height);
		}

		// Inflate and Offset
		public Rect Inflate(Size sz) => Inflate(sz.Width, sz.Height);

		public Rect Inflate(double width, double height)
		{
			Rect r = this;
			r.X -= width;
			r.Y -= height;
			r.Width += width * 2;
			r.Height += height * 2;
			return r;
		}

		public Rect Offset(double dx, double dy)
		{
			Rect r = this;
			r.X += dx;
			r.Y += dy;
			return r;
		}

		public Rect Offset(Point dr) => Offset(dr.X, dr.Y);

		public Rect Round() => new Rect(Math.Round(X), Math.Round(Y), Math.Round(Width), Math.Round(Height));

		public void Deconstruct(out double x, out double y, out double width, out double height)
		{
			x = X;
			y = Y;
			width = Width;
			height = Height;
		}

		public override string ToString() => $"{{X={X.ToString(CultureInfo.InvariantCulture)} Y={Y.ToString(CultureInfo.InvariantCulture)} Width={Width.ToString(CultureInfo.InvariantCulture)} Height={Height.ToString(CultureInfo.InvariantCulture)}}}";
	}
}