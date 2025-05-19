using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a rectangle with single-precision floating-point x, y coordinates and width and height.
	/// </summary>
	[DebuggerDisplay("X={X}, Y={Y}, Width={Width}, Height={Height}")]
	[TypeConverter(typeof(Converters.RectFTypeConverter))]
	public partial struct RectF
	{
		/// <summary>
		/// Gets or sets the x-coordinate of the rectangle's left edge.
		/// </summary>
		public float X { get; set; }

		/// <summary>
		/// Gets or sets the y-coordinate of the rectangle's top edge.
		/// </summary>
		public float Y { get; set; }

		/// <summary>
		/// Gets or sets the width of the rectangle.
		/// </summary>
		public float Width { get; set; }

		/// <summary>
		/// Gets or sets the height of the rectangle.
		/// </summary>
		public float Height { get; set; }

		/// <summary>
		/// Represents a <see cref="RectF"/> with all values set to 0.
		/// </summary>
		public static RectF Zero = new RectF();

		public override string ToString()
		{
			return string.Format("{{X={0} Y={1} Width={2} Height={3}}}", X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture), Width.ToString(CultureInfo.InvariantCulture),
				Height.ToString(CultureInfo.InvariantCulture));
		}

		// constructors
		public RectF(float x, float y, float width, float height) : this()
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public RectF(PointF loc, SizeF sz) : this(loc.X, loc.Y, sz.Width, sz.Height)
		{
		}

		public static RectF FromLTRB(float left, float top, float right, float bottom)
		{
			return new RectF(left, top, right - left, bottom - top);
		}

		public bool Equals(RectF other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
		}

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			return obj is RectF && Equals((RectF)obj);
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

		public static bool operator ==(RectF r1, RectF r2)
		{
			return r1.Equals(r2);
		}

		public static bool operator !=(RectF r1, RectF r2)
		{
			return !(r1 == r2);
		}

		// Hit Testing / Intersection / Union
		public bool Contains(RectF rect)
		{
			return X <= rect.X && Right >= rect.Right && Y <= rect.Y && Bottom >= rect.Bottom;
		}

		public bool Contains(PointF pt)
		{
			return Contains(pt.X, pt.Y);
		}

		public bool Contains(float x, float y)
		{
			return (x >= Left) && (x < Right) && (y >= Top) && (y < Bottom);
		}

		public bool IntersectsWith(RectF r)
		{
			return !((Left >= r.Right) || (Right <= r.Left) || (Top >= r.Bottom) || (Bottom <= r.Top));
		}

		public RectF Union(RectF r)
		{
			return Union(this, r);
		}

		public static RectF Union(RectF r1, RectF r2)
		{
			return FromLTRB(Math.Min(r1.Left, r2.Left), Math.Min(r1.Top, r2.Top), Math.Max(r1.Right, r2.Right), Math.Max(r1.Bottom, r2.Bottom));
		}

		public RectF Intersect(RectF r)
		{
			return Intersect(this, r);
		}

		public static RectF Intersect(RectF r1, RectF r2)
		{
			float x = Math.Max(r1.X, r2.X);
			float y = Math.Max(r1.Y, r2.Y);
			float width = Math.Min(r1.Right, r2.Right) - x;
			float height = Math.Min(r1.Bottom, r2.Bottom) - y;

			if (width < 0 || height < 0)
			{
				return Zero;
			}
			return new RectF(x, y, width, height);
		}

		// Position/Size
		public float Top
		{
			get => Y;
			set => Y = value;
		}

		public float Bottom
		{
			get => Y + Height;
			set => Height = value - Y;
		}

		public float Right
		{
			get => X + Width;
			set => Width = value - X;
		}

		public float Left
		{
			get => X;
			set => X = value;
		}

		public bool IsEmpty => (Width <= 0) || (Height <= 0);

		public SizeF Size
		{
			get => new SizeF(Width, Height);
			set
			{
				Width = value.Width;
				Height = value.Height;
			}
		}

		public PointF Location
		{
			get => new PointF(X, Y);
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}

		public PointF Center => new PointF(X + Width / 2, Y + Height / 2);

		// Inflate and Offset
		public RectF Inflate(SizeF sz)
		{
			return Inflate(sz.Width, sz.Height);
		}

		public RectF Inflate(float width, float height)
		{
			RectF r = this;
			r.X -= width;
			r.Y -= height;
			r.Width += width * 2;
			r.Height += height * 2;
			return r;
		}

		public RectF Offset(float dx, float dy)
		{
			RectF r = this;
			r.X += dx;
			r.Y += dy;
			return r;
		}

		public RectF Offset(PointF dr)
		{
			return Offset(dr.X, dr.Y);
		}

		public RectF Round()
		{
			return new RectF(MathF.Round(X), MathF.Round(Y), MathF.Round(Width), MathF.Round(Height));
		}

		public void Deconstruct(out float x, out float y, out float width, out float height)
		{
			x = X;
			y = Y;
			width = Width;
			height = Height;
		}

		public static implicit operator Rect(RectF rect) => new Rect(rect.X, rect.Y, rect.Width, rect.Height);

		public static bool TryParse(string value, out RectF rectangleF)
		{
			if (!string.IsNullOrEmpty(value))
			{
				string[] xywh = value.Split(',');
				if (xywh.Length == 4
					&& float.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out float x)
					&& float.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out float y)
					&& float.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out float w)
					&& float.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out float h))
				{
					rectangleF = new RectF(x, y, w, h);
					return true;
				}
			}

			rectangleF = default;
			return false;
		}
	}
}
