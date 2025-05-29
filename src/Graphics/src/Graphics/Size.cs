using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a size with double-precision floating-point width and height.
	/// </summary>
	[DebuggerDisplay("Width={Width}, Height={Height}")]
	[TypeConverter(typeof(Converters.SizeTypeConverter))]
	public partial struct Size
	{
		/// <summary>
		/// A size with both width and height set to zero.
		/// </summary>
		public static readonly Size Zero;

		/// <summary>
		/// Initializes a new instance of the <see cref="Size"/> struct with equal width and height.
		/// </summary>
		/// <param name="size">The value to use for both width and height (default is 0).</param>
		public Size(double size = 0)
		{
			Width = size;
			Height = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Size"/> struct with the specified width and height.
		/// </summary>
		/// <param name="width">The width component of the size.</param>
		/// <param name="height">The height component of the size.</param>
		public Size(double width, double height)
		{
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Size"/> struct from a <see cref="Vector2"/>.
		/// </summary>
		/// <param name="vector">The vector containing the width (X) and height (Y) values.</param>
		public Size(Vector2 vector)
		{
			Width = vector.X;
			Height = vector.Y;
		}

		/// <summary>
		/// Gets a value indicating whether both the width and height of this size are zero.
		/// </summary>
		public bool IsZero => Width == 0 && Height == 0;

		/// <summary>
		/// Gets or sets the width component of this size.
		/// </summary>
		[DefaultValue(0d)]
		public double Width { get; set; }

		/// <summary>
		/// Gets or sets the height component of this size.
		/// </summary>
		[DefaultValue(0d)]
		public double Height { get; set; }

		/// <summary>
		/// Adds two sizes together by adding their width and height components.
		/// </summary>
		/// <param name="s1">The first size to add.</param>
		/// <param name="s2">The second size to add.</param>
		/// <returns>A new size with the width and height being the sum of the two sizes.</returns>
		public static Size operator +(Size s1, Size s2)
		{
			return new Size(s1.Width + s2.Width, s1.Height + s2.Height);
		}

		/// <summary>
		/// Subtracts one size from another by subtracting their width and height components.
		/// </summary>
		/// <param name="s1">The size to subtract from.</param>
		/// <param name="s2">The size to subtract.</param>
		/// <returns>A new size with the width and height being the difference of the two sizes.</returns>
		public static Size operator -(Size s1, Size s2)
		{
			return new Size(s1.Width - s2.Width, s1.Height - s2.Height);
		}

		/// <summary>
		/// Multiplies a size by a scalar value.
		/// </summary>
		/// <param name="s1">The size to multiply.</param>
		/// <param name="value">The scalar value to multiply by.</param>
		/// <returns>A new size with the width and height multiplied by the scalar value.</returns>
		public static Size operator *(Size s1, double value)
		{
			return new Size(s1.Width * value, s1.Height * value);
		}

		/// <summary>
		/// Divides a size by a scalar value.
		/// </summary>
		/// <param name="s1">The size to divide.</param>
		/// <param name="value">The scalar value to divide by.</param>
		/// <returns>A new size with the width and height divided by the scalar value.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown when <paramref name="value"/> is zero.</exception>
		public static Size operator /(Size s1, double value)
		{
			return new Size(s1.Width / value, s1.Height / value);
		}

		/// <summary>
		/// Determines whether two sizes have the same width and height.
		/// </summary>
		/// <param name="s1">The first size to compare.</param>
		/// <param name="s2">The second size to compare.</param>
		/// <returns><c>true</c> if the sizes have the same width and height; otherwise, <c>false</c>.</returns>
		public static bool operator ==(Size s1, Size s2)
		{
			return s1.Width == s2.Width && s1.Height == s2.Height;
		}

		/// <summary>
		/// Determines whether two sizes have different width or height.
		/// </summary>
		/// <param name="s1">The first size to compare.</param>
		/// <param name="s2">The second size to compare.</param>
		/// <returns><c>true</c> if the sizes have different width or height; otherwise, <c>false</c>.</returns>
		public static bool operator !=(Size s1, Size s2)
		{
			return s1.Width != s2.Width || s1.Height != s2.Height;
		}

		/// <summary>
		/// Explicitly converts a <see cref="Size"/> to a <see cref="Point"/>.
		/// </summary>
		/// <param name="size">The size to convert.</param>
		/// <returns>A new point with X and Y values set to the size's width and height.</returns>
		public static explicit operator Point(Size size)
		{
			return new Point(size.Width, size.Height);
		}

		/// <summary>
		/// Determines whether the specified size is equal to the current size.
		/// </summary>
		/// <param name="other">The size to compare with the current size.</param>
		/// <returns><c>true</c> if the specified size is equal to the current size; otherwise, <c>false</c>.</returns>
		public bool Equals(Size other)
		{
			return Width.Equals(other.Width) && Height.Equals(other.Height);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current size.
		/// </summary>
		/// <param name="obj">The object to compare with the current size.</param>
		/// <returns><c>true</c> if the specified object is a <see cref="Size"/> and has the same width and height as the current size; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			return obj is Size && Equals((Size)obj);
		}

		/// <summary>
		/// Returns a hash code for this size.
		/// </summary>
		/// <returns>A hash code for the current size.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
			}
		}

		/// <summary>
		/// Returns a string representation of this size.
		/// </summary>
		/// <returns>A string in the format "{Width=w Height=h}" with invariant culture formatting.</returns>
		public override string ToString()
		{
			return string.Format("{{Width={0} Height={1}}}", Width.ToString(CultureInfo.InvariantCulture), Height.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Deconstructs this size into its component parts.
		/// </summary>
		/// <param name="width">When this method returns, contains the width component of this size.</param>
		/// <param name="height">When this method returns, contains the height component of this size.</param>
		public void Deconstruct(out double width, out double height)
		{
			width = Width;
			height = Height;
		}

		/// <summary>
		/// Implicitly converts a <see cref="Size"/> to a <see cref="SizeF"/>.
		/// </summary>
		/// <param name="s">The size to convert.</param>
		/// <returns>A new <see cref="SizeF"/> with the width and height converted to float.</returns>
		public static implicit operator SizeF(Size s) => new SizeF((float)s.Width, (float)s.Height);

		public static bool TryParse(string value, out Size size)
		{
			if (!string.IsNullOrEmpty(value))
			{
				string[] wh = value.Split(',');
				if (wh.Length == 2
					&& double.TryParse(wh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
					&& double.TryParse(wh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
				{
					size = new Size(w, h);
					return true;
				}
			}

			size = default;
			return false;
		}
	}
}
