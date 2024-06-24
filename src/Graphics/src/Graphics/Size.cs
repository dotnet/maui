using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	[DebuggerDisplay("Width={Width}, Height={Height}")]
	[TypeConverter(typeof(Converters.SizeTypeConverter))]
	public partial struct Size
	{
		public static readonly Size Zero;

		public Size(double size = 0)
		{
			Width = size;
			Height = size;
		}

		public Size(double width, double height)
		{
			Width = width;
			Height = height;
		}

		public Size(Vector2 vector)
		{
			Width = vector.X;
			Height = vector.Y;
		}

		public bool IsZero => Width == 0 && Height == 0;

		[DefaultValue(0d)]
		public double Width { get; set; }

		[DefaultValue(0d)]
		public double Height { get; set; }

		public static Size operator +(Size s1, Size s2)
		{
			return new Size(s1.Width + s2.Width, s1.Height + s2.Height);
		}

		public static Size operator -(Size s1, Size s2)
		{
			return new Size(s1.Width - s2.Width, s1.Height - s2.Height);
		}

		public static Size operator *(Size s1, double value)
		{
			return new Size(s1.Width * value, s1.Height * value);
		}

		public static Size operator /(Size s1, double value)
		{
			return new Size(s1.Width / value, s1.Height / value);
		}

		public static bool operator ==(Size s1, Size s2)
		{
			return s1.Width == s2.Width && s1.Height == s2.Height;
		}

		public static bool operator !=(Size s1, Size s2)
		{
			return s1.Width != s2.Width || s1.Height != s2.Height;
		}

		public static explicit operator Point(Size size)
		{
			return new Point(size.Width, size.Height);
		}

		public bool Equals(Size other)
		{
			return Width.Equals(other.Width) && Height.Equals(other.Height);
		}

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			return obj is Size && Equals((Size)obj);
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

		public void Deconstruct(out double width, out double height)
		{
			width = Width;
			height = Height;
		}
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
