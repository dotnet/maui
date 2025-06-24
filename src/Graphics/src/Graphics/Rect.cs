using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a rectangle with double-precision floating-point x, y coordinates and width and height.
	/// </summary>
	[DebuggerDisplay("X={X}, Y={Y}, Width={Width}, Height={Height}")]
	[TypeConverter(typeof(Converters.RectTypeConverter))]
	public partial struct Rect
	{
		/// <summary>
		/// Gets or sets the x-coordinate of the rectangle's left edge.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Gets or sets the y-coordinate of the rectangle's top edge.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Gets or sets the width of the rectangle.
		/// </summary>
		public double Width { get; set; }

		/// <summary>
		/// Gets or sets the height of the rectangle.
		/// </summary>
		public double Height { get; set; }

		/// <summary>
		/// A rectangle with all values (X, Y, Width, Height) set to zero.
		/// </summary>
		public static Rect Zero = new Rect();

		/// <summary>
		/// Returns a string representation of the rectangle.
		/// </summary>
		/// <returns>A string in the format "X=x Y=y Width=width Height=height" with invariant culture formatting.</returns>
		public override string ToString()
		{
			return string.Format("{{X={0} Y={1} Width={2} Height={3}}}", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture), Width.ToString(CultureInfo.InvariantCulture), Height.ToString(CultureInfo.InvariantCulture));
		}

		// constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Rect"/> struct with the specified position and dimensions.
		/// </summary>
		/// <param name="x">The x-coordinate of the rectangle's left edge.</param>
		/// <param name="y">The y-coordinate of the rectangle's top edge.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
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

		public static Rect FromLTRB(double left, double top, double right, double bottom)
		{
			return new Rect(left, top, right - left, bottom - top);
		}

		public bool Equals(Rect other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
		}

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			return obj is Rect && Equals((Rect)obj);
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

		public static bool operator ==(Rect r1, Rect r2)
		{
			return r1.Equals(r2);
		}

		public static bool operator !=(Rect r1, Rect r2)
		{
			return !(r1 == r2);
		}

		// Hit Testing / Intersection / Union
		public bool Contains(Rect rect)
		{
			return X <= rect.X && Right >= rect.Right && Y <= rect.Y && Bottom >= rect.Bottom;
		}

		public bool Contains(Point pt)
		{
			return Contains(pt.X, pt.Y);
		}

		public bool Contains(double x, double y)
		{
			return (x >= Left) && (x < Right) && (y >= Top) && (y < Bottom);
		}

		public bool IntersectsWith(Rect r)
		{
			return !((Left >= r.Right) || (Right <= r.Left) || (Top >= r.Bottom) || (Bottom <= r.Top));
		}

		public Rect Union(Rect r)
		{
			return Union(this, r);
		}

		public static Rect Union(Rect r1, Rect r2)
		{
			return FromLTRB(Math.Min(r1.Left, r2.Left), Math.Min(r1.Top, r2.Top), Math.Max(r1.Right, r2.Right), Math.Max(r1.Bottom, r2.Bottom));
		}

		public Rect Intersect(Rect r)
		{
			return Intersect(this, r);
		}

		public static Rect Intersect(Rect r1, Rect r2)
		{
			double x = Math.Max(r1.X, r2.X);
			double y = Math.Max(r1.Y, r2.Y);
			double width = Math.Min(r1.Right, r2.Right) - x;
			double height = Math.Min(r1.Bottom, r2.Bottom) - y;

			if (width < 0 || height < 0)
			{
				return Zero;
			}
			return new Rect(x, y, width, height);
		}

		// Position/Size
		public double Top
		{
			get => Y;
			set => Y = value;
		}

		public double Bottom
		{
			get => Y + Height;
			set => Height = value - Y;
		}

		public double Right
		{
			get => X + Width;
			set => Width = value - X;
		}

		public double Left
		{
			get => X;
			set => X = value;
		}

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

		// Inflate and Offset
		public Rect Inflate(Size sz)
		{
			return Inflate(sz.Width, sz.Height);
		}

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

		public Rect Offset(Point dr)
		{
			return Offset(dr.X, dr.Y);
		}

		public Rect Round()
		{
			return new Rect(Math.Round(X), Math.Round(Y), Math.Round(Width), Math.Round(Height));
		}

		public void Deconstruct(out double x, out double y, out double width, out double height)
		{
			x = X;
			y = Y;
			width = Width;
			height = Height;
		}

		public static implicit operator RectF(Rect rect) => new RectF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);

		public static bool TryParse(string value, out Rect rectangle)
		{
			// IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
			if (!string.IsNullOrEmpty(value))
			{
				string[] xywh = value.Split(',');
				if (xywh.Length == 4
					&& double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x)
					&& double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y)
					&& double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
					&& double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
				{
					rectangle = new Rect(x, y, w, h);
					return true;
				}
			}

			rectangle = default;
			return false;
		}
	}
}
