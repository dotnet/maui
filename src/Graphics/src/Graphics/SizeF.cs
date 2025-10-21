using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a size with single-precision floating-point width and height.
	/// </summary>
	[DebuggerDisplay("Width={Width}, Height={Height}")]
	[TypeConverter(typeof(Converters.SizeFTypeConverter))]
	public partial struct SizeF
	{
		/// <summary>
		/// A size with both width and height set to zero.
		/// </summary>
		public static readonly SizeF Zero;

		/// <summary>
		/// Initializes a new instance of the <see cref="SizeF"/> struct with the specified value for both width and height.
		/// </summary>
		/// <param name="size">The value to use for both width and height.</param>
		public SizeF(float size = 0)
		{
			Width = size;
			Height = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SizeF"/> struct with the specified width and height.
		/// </summary>
		/// <param name="width">The width of the size.</param>
		/// <param name="height">The height of the size.</param>
		public SizeF(float width, float height)
		{
			Width = width;
			Height = height;
		}

		public SizeF(Vector2 vector)
		{
			Width = vector.X;
			Height = vector.Y;
		}

		public bool IsZero => Width == 0 && Height == 0;

		[DefaultValue(0f)]
		public float Width { get; set; }

		[DefaultValue(0f)]
		public float Height { get; set; }

		public SizeF TransformNormalBy(in Matrix3x2 transform)
		{
			return (SizeF)Vector2.TransformNormal((Vector2)this, transform);
		}

		public static SizeF operator +(SizeF s1, SizeF s2)
		{
			return new SizeF(s1.Width + s2.Width, s1.Height + s2.Height);
		}

		public static SizeF operator -(SizeF s1, SizeF s2)
		{
			return new SizeF(s1.Width - s2.Width, s1.Height - s2.Height);
		}

		public static SizeF operator *(SizeF s1, float value)
		{
			return new SizeF(s1.Width * value, s1.Height * value);
		}

		public static SizeF operator /(SizeF s1, float value)
		{
			return new SizeF(s1.Width / value, s1.Height / value);
		}

		public static bool operator ==(SizeF s1, SizeF s2)
		{
			return s1.Width == s2.Width && s1.Height == s2.Height;
		}

		public static bool operator !=(SizeF s1, SizeF s2)
		{
			return s1.Width != s2.Width || s1.Height != s2.Height;
		}

		public static explicit operator PointF(SizeF size)
		{
			return new PointF(size.Width, size.Height);
		}

		public static explicit operator Vector2(SizeF size)
		{
			return new Vector2(size.Width, size.Height);
		}

		public static explicit operator SizeF(Vector2 size)
		{
			return new SizeF(size.X, size.Y);
		}

		public bool Equals(SizeF other)
		{
			return Width.Equals(other.Width) && Height.Equals(other.Height);
		}

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			return obj is SizeF && Equals((SizeF)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("{{Width={0} Height={1}}}", Width.ToString(CultureInfo.InvariantCulture), Height.ToString(CultureInfo.InvariantCulture));
		}

		public void Deconstruct(out float width, out float height)
		{
			width = Width;
			height = Height;
		}

		public static implicit operator Size(SizeF s) => new Size(s.Width, s.Height);

		public static bool TryParse(string value, out SizeF sizeF)
		{
			if (!string.IsNullOrEmpty(value))
			{
				string[] wh = value.Split(',');
				if (wh.Length == 2
					&& double.TryParse(wh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
					&& double.TryParse(wh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
				{
					sizeF = new Size(w, h);
					return true;
				}
			}

			sizeF = default;
			return false;
		}
	}
}
