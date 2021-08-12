using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public struct Shadow
	{
		float _opacity;
		float _radius;
		Size _offset;
		Color _color;

		public static Shadow Zero = new Shadow();

		public bool IsEmpty
		{
			get { return Radius == 0 && Opacity == 0 && Color == null && Offset == Size.Zero; }
		}

		[DefaultValue(1f)]
		public float Radius
		{
			get { return _radius; }
			set
			{
				if (double.IsNaN(value))
					throw new ArgumentException("NaN is not a valid value for Radius");
				_radius = value;
			}
		}

		[DefaultValue(10f)]
		public float Opacity
		{
			get { return _opacity; }
			set
			{
				if (double.IsNaN(value))
					throw new ArgumentException("NaN is not a valid value for Opacity");
				_opacity = value;
			}
		}

		public Color Color
		{
			get { return _color; }
			set { _color = value; }
		}

		public Size Offset
		{
			get { return _offset; }
			set { _offset = value; }
		}

		public bool Equals(Shadow other)
		{
			return
				_radius.Equals(other._radius) &&
				_opacity.Equals(other._opacity) &&
				_offset.Equals(other._offset) &&
				_color.Equals(other._color);
		}

		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;
			return obj is Shadow shadow && Equals(shadow);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_radius.GetHashCode() * 397) ^ _opacity.GetHashCode();
			}
		}

		public override string ToString()
		{
			return string.Format("{{Opacity={0} Radius={1} Offset={2} Color={3}}}",
				_opacity.ToString(CultureInfo.InvariantCulture),
				_radius.ToString(CultureInfo.InvariantCulture),
				_offset.ToString(),
				_color.ToHex());
		}
	}
}