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
		double _width;
		double _height;

		public static readonly Size Zero;

		public Size(double size = 0)
		{
			if (double.IsNaN(size))
				throw new ArgumentException("NaN is not a valid value for size");
			_width = size;
			_height = size;
		}

		public Size(double width, double height)
		{
			if (double.IsNaN(width))
				throw new ArgumentException("NaN is not a valid value for width");
			if (double.IsNaN(height))
				throw new ArgumentException("NaN is not a valid value for height");
			_width = width;
			_height = height;
		}

		public Size(Vector2 vector)
		{
			if (float.IsNaN(vector.X))
				throw new ArgumentException("NaN is not a valid value for X");
			if (float.IsNaN(vector.Y))
				throw new ArgumentException("NaN is not a valid value for Y");
			_width = vector.X;
			_height = vector.Y;
		}

		public bool IsZero => _width == 0 && _height == 0;

		[DefaultValue(0d)]
		public double Width
		{
			get => _width;
			set
			{
				if (double.IsNaN(value))
					throw new ArgumentException("NaN is not a valid value for Width");
				_width = value;
			}
		}

		[DefaultValue(0d)]
		public double Height
		{
			get => _height;
			set
			{
				if (double.IsNaN(value))
					throw new ArgumentException("NaN is not a valid value for Height");
				_height = value;
			}
		}

		public static Size operator +(Size s1, Size s2)
		{
			return new Size(s1._width + s2._width, s1._height + s2._height);
		}

		public static Size operator -(Size s1, Size s2)
		{
			return new Size(s1._width - s2._width, s1._height - s2._height);
		}

		public static Size operator *(Size s1, double value)
		{
			return new Size(s1._width * value, s1._height * value);
		}

		public static Size operator /(Size s1, double value)
		{
			return new Size(s1._width / value, s1._height / value);
		}

		public static bool operator ==(Size s1, Size s2)
		{
			return s1._width == s2._width && s1._height == s2._height;
		}

		public static bool operator !=(Size s1, Size s2)
		{
			return s1._width != s2._width || s1._height != s2._height;
		}

		public static explicit operator Point(Size size)
		{
			return new Point(size.Width, size.Height);
		}

		public bool Equals(Size other)
		{
			return _width.Equals(other._width) && _height.Equals(other._height);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is Size && Equals((Size)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_width.GetHashCode() * 397) ^ _height.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("{{Width={0} Height={1}}}", _width.ToString(CultureInfo.InvariantCulture), _height.ToString(CultureInfo.InvariantCulture));
		}

		public void Deconstruct(out double width, out double height)
		{
			width = Width;
			height = Height;
		}
		public static implicit operator SizeF(Size s) => new SizeF((float)s.Width,(float)s.Height);

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
