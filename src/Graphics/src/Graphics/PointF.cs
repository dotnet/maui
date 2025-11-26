using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a point in 2D space using single-precision floating-point coordinates.
	/// </summary>
	[DebuggerDisplay("X={X}, Y={Y}")]
	[TypeConverter(typeof(Converters.PointFTypeConverter))]
	public partial struct PointF
	{
		/// <summary>
		/// Gets or sets the X-coordinate of this point.
		/// </summary>
		public float X { get; set; }

		/// <summary>
		/// Gets or sets the Y-coordinate of this point.
		/// </summary>
		public float Y { get; set; }

		/// <summary>
		/// Represents a <see cref="PointF"/> with coordinates (0,0).
		/// </summary>
		public static readonly PointF Zero = new PointF();

		/// <summary>
		/// Returns a string representation of this <see cref="PointF"/>.
		/// </summary>
		/// <returns>A string in the format "X=0 Y=0" (values may vary).</returns>
		public override string ToString()
		{
			return $"{{X={X.ToString(CultureInfo.InvariantCulture)} Y={Y.ToString(CultureInfo.InvariantCulture)}}}";
		}

		public PointF(float x, float y) : this()
		{
			X = x;
			Y = y;
		}

		public PointF(SizeF sz) : this()
		{
			X = sz.Width;
			Y = sz.Height;
		}

		public PointF(Vector2 v)
		{
			X = v.X;
			Y = v.Y;
		}

		public override bool Equals(object o)
		{
			if (!(o is PointF))
				return false;

			return this == (PointF)o;
		}

		public bool Equals(object o, float epsilon)
		{
			if (!(o is PointF))
				return false;

			var compareTo = (PointF)o;
			return Math.Abs(compareTo.X - X) < epsilon && Math.Abs(compareTo.Y - Y) < epsilon;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() * 397);
		}

		public PointF Offset(float dx, float dy)
		{
			PointF p = this;
			p.X += dx;
			p.Y += dy;
			return p;
		}

		public PointF TransformBy(in Matrix3x2 transform)
		{
			return Vector2.Transform((Vector2)this, transform);
		}

		public PointF Round()
		{
			return new PointF(MathF.Round(X), MathF.Round(Y));
		}

		public bool IsEmpty => X == 0 && Y == 0;

		public static explicit operator SizeF(PointF pt)
		{
			return new SizeF(pt.X, pt.Y);
		}

		public static PointF operator +(PointF pt, SizeF sz)
		{
			return new PointF(pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static SizeF operator -(PointF ptA, PointF ptB)
		{
			return new SizeF(ptA.X - ptB.X, ptA.Y - ptB.Y);
		}

		public static PointF operator -(PointF pt, SizeF sz)
		{
			return new PointF(pt.X - sz.Width, pt.Y - sz.Height);
		}

		public static bool operator ==(PointF ptA, PointF ptB)
		{
			return ptA.X == ptB.X && ptA.Y == ptB.Y;
		}

		public static bool operator !=(PointF ptA, PointF ptB)
		{
			return ptA.X != ptB.X || ptA.Y != ptB.Y;
		}

		public float Distance(PointF other)
		{
			return MathF.Sqrt(MathF.Pow(X - other.X, 2) + MathF.Pow(Y - other.Y, 2));
		}

		public void Deconstruct(out float x, out float y)
		{
			x = X;
			y = Y;
		}
		public static implicit operator Point(PointF p) => new Point(p.X, p.Y);

		public static implicit operator PointF(Vector2 v) => new PointF(v);

		public static explicit operator Vector2(PointF p) => new Vector2(p.X, p.Y);

		public static bool TryParse(string value, out PointF pointF)
		{
			if (!string.IsNullOrEmpty(value))
			{
				string[] xy = value.Split(',');
				if (xy.Length == 2 && float.TryParse(xy[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var x)
					&& float.TryParse(xy[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var y))
				{
					pointF = new PointF(x, y);
					return true;
				}
			}

			pointF = default;
			return false;
		}
	}
}
