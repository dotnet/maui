using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
    [DebuggerDisplay("Width={Width}, Height={Height}")]
    public struct SizeF
    {
        float _width;
        float _height;

        public static readonly SizeF Zero;

        public SizeF(float size = 0)
        {
            if (float.IsNaN(size))
                throw new ArgumentException("NaN is not a valid value for size");
            _width = size;
            _height = size;
        }

        public SizeF(float width, float height)
        {
            if (float.IsNaN(width))
                throw new ArgumentException("NaN is not a valid value for width");
            if (float.IsNaN(height))
                throw new ArgumentException("NaN is not a valid value for height");
            _width = width;
            _height = height;
        }

        public bool IsZero => _width == 0 && _height == 0;

        [DefaultValue(0d)]
        public float Width
        {
            get => _width;
            set
            {
                if (float.IsNaN(value))
                    throw new ArgumentException("NaN is not a valid value for Width");
                _width = value;
            }
        }

        [DefaultValue(0d)]
        public float Height
        {
            get => _height;
            set
            {
                if (float.IsNaN(value))
                    throw new ArgumentException("NaN is not a valid value for Height");
                _height = value;
            }
        }

        public static SizeF operator +(SizeF s1, SizeF s2)
        {
            return new SizeF(s1._width + s2._width, s1._height + s2._height);
        }

        public static SizeF operator -(SizeF s1, SizeF s2)
        {
            return new SizeF(s1._width - s2._width, s1._height - s2._height);
        }

        public static SizeF operator *(SizeF s1, float value)
        {
            return new SizeF(s1._width * value, s1._height * value);
        }

        public static bool operator ==(SizeF s1, SizeF s2)
        {
            return s1._width == s2._width && s1._height == s2._height;
        }

        public static bool operator !=(SizeF s1, SizeF s2)
        {
            return s1._width != s2._width || s1._height != s2._height;
        }

        public static explicit operator PointF(SizeF size)
        {
            return new PointF(size.Width, size.Height);
        }

        public bool Equals(SizeF other)
        {
            return _width.Equals(other._width) && _height.Equals(other._height);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is SizeF && Equals((SizeF)obj);
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

        public void Deconstruct(out float width, out float height)
        {
            width = Width;
            height = Height;
        }
    }
}