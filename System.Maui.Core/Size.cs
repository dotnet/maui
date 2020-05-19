using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Xamarin.Forms
{
	[DebuggerDisplay("Width={Width}, Height={Height}")]
	[TypeConverter(typeof(SizeTypeConverter))]
	public struct Size
	{
		double _width;
		double _height;

		public static readonly Size Zero;

		public Size(double width, double height)
		{
			if (double.IsNaN(width))
				throw new ArgumentException("NaN is not a valid value for width");
			if (double.IsNaN(height))
				throw new ArgumentException("NaN is not a valid value for height");
			_width = width;
			_height = height;
		}

		public bool IsZero
		{
			get { return (_width == 0) && (_height == 0); }
		}

		[DefaultValue(0d)]
		public double Width
		{
			get { return _width; }
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
			get { return _height; }
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

		public static bool operator ==(Size s1, Size s2)
		{
			return (s1._width == s2._width) && (s1._height == s2._height);
		}

		public static bool operator !=(Size s1, Size s2)
		{
			return (s1._width != s2._width) || (s1._height != s2._height);
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
	}
}