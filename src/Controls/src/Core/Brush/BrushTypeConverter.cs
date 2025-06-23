#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BrushTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.BrushTypeConverter']/Docs/*" />
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.BrushTypeConverter")]
	public class BrushTypeConverter : TypeConverter
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/BrushTypeConverter.xml" path="//Member[@MemberName='LinearGradient']/Docs/*" />
		public const string LinearGradient = "linear-gradient";
		/// <include file="../../docs/Microsoft.Maui.Controls/BrushTypeConverter.xml" path="//Member[@MemberName='RadialGradient']/Docs/*" />
		public const string RadialGradient = "radial-gradient";
		/// <include file="../../docs/Microsoft.Maui.Controls/BrushTypeConverter.xml" path="//Member[@MemberName='Rgb']/Docs/*" />
		public const string Rgb = "rgb";
		/// <include file="../../docs/Microsoft.Maui.Controls/BrushTypeConverter.xml" path="//Member[@MemberName='Rgba']/Docs/*" />
		public const string Rgba = "rgba";
		/// <include file="../../docs/Microsoft.Maui.Controls/BrushTypeConverter.xml" path="//Member[@MemberName='Hsl']/Docs/*" />
		public const string Hsl = "hsl";
		/// <include file="../../docs/Microsoft.Maui.Controls/BrushTypeConverter.xml" path="//Member[@MemberName='Hsla']/Docs/*" />
		public const string Hsla = "hsla";

		readonly ColorTypeConverter _colorTypeConverter = new ColorTypeConverter();

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string)
				|| sourceType == typeof(Color)
				|| sourceType == typeof(Paint);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(Paint);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is Color colorValue)
			{
				return (Brush)colorValue;
			}
			else if (value is Paint paintValue)
			{
				return (Brush)paintValue;
			}

			var strValue = value?.ToString();

			if (strValue != null)
			{
				strValue = strValue.Trim();

				if (strValue.StartsWith(LinearGradient) || strValue.StartsWith(RadialGradient))
				{
					var gradientBrushParser = new GradientBrushParser(_colorTypeConverter);
					var brush = gradientBrushParser.Parse(strValue);

					if (brush != null)
						return brush;
				}

				if (strValue.StartsWith(Rgb, StringComparison.InvariantCulture) || strValue.StartsWith(Rgba, StringComparison.InvariantCulture) || strValue.StartsWith(Hsl, StringComparison.InvariantCulture) || strValue.StartsWith(Hsla))
				{
					var color = (Color)_colorTypeConverter.ConvertFromInvariantString(strValue);
					return new SolidColorBrush(color);
				}
			}

			string[] parts = strValue.Split('.');

			if (parts.Length == 1 || (parts.Length == 2 && parts[0] == "Color"))
			{
				var color = (Color)_colorTypeConverter.ConvertFromInvariantString(strValue);
				return new SolidColorBrush(color);
			}

			return new SolidColorBrush(null);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is Brush brush && destinationType == typeof(Paint))
			{
				return (Paint)brush;
			}

			throw new NotSupportedException();
		}

		public class GradientBrushParser
		{
			readonly ColorTypeConverter _colorConverter;
			GradientBrush _gradient;
			string[] _parts;
			int _position;

			public GradientBrushParser(ColorTypeConverter colorConverter = null)
			{
				_colorConverter = colorConverter ?? new ColorTypeConverter();
			}

			public GradientBrush Parse(string css)
			{
				if (string.IsNullOrWhiteSpace(css))
				{
					return _gradient;
				}

#if NETSTANDARD2_0
				_parts = css.Replace("\r\n", "")
#else
				_parts = css.Replace("\r\n", "", StringComparison.Ordinal)
#endif
					.Split(new[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

				while (_position < _parts.Length)
				{
					var part = GetPart().Trim();

					// Hex Color
					if (part.StartsWith("#", StringComparison.Ordinal))
					{
						var parts = part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						var color = (Color)_colorConverter.ConvertFromInvariantString(parts[0]);

						if (TryParseOffsets(parts, out var offsets))
							AddGradientStops(color, offsets);
						else
							AddGradientStop(color);
					}

					// Color by name
					var colorParts = part.Split('.');
					if (colorParts[0].Equals("Color", StringComparison.Ordinal))
					{
						var parts = part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						var color = (Color)_colorConverter.ConvertFromInvariantString(parts[0]);

						if (TryParseOffsets(parts, out var offsets))
							AddGradientStops(color, offsets);
						else
							AddGradientStop(color);
					}

					// Color (Rgb, Rgba, Hsl, Hsla)
					if (part.Equals(Rgb, StringComparison.OrdinalIgnoreCase)
						|| part.Equals(Rgba, StringComparison.OrdinalIgnoreCase)
						|| part.Equals(Hsl, StringComparison.OrdinalIgnoreCase)
						|| part.Equals(Hsla, StringComparison.OrdinalIgnoreCase))
					{
						var colorString = new StringBuilder(part);

						colorString.Append('(');
						colorString.Append(GetNextPart());
						colorString.Append(',');
						colorString.Append(GetNextPart());
						colorString.Append(',');
						colorString.Append(GetNextPart());

						if (part == Rgba || part == Hsla)
						{
							colorString.Append(',');
							colorString.Append(GetNextPart());
						}

						colorString.Append(')');

						var color = (Color)_colorConverter.ConvertFromInvariantString(colorString.ToString());
						var parts = GetNextPart().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

						if (TryParseOffsets(parts, out var offsets))
							AddGradientStops(color, offsets);
						else
						{
							AddGradientStop(color);
							_position--;
						}
					}

					// LinearGradient
					if (part == LinearGradient)
					{
						var direction = GetNextPart().Trim();
						var hasAngle = TryParseAngle(direction, out var angle);

						if (hasAngle)
							CreateLinearGradient(angle);
						else
						{
							CreateLinearGradient(0);
							_position--;
						}
					}

					// RadialGradient
					if (part == RadialGradient)
					{
						var center = GetGradientCenter();
						CreateRadialGradient(center);
					}

					_position++;
				}

				return _gradient;
			}

			string GetPart()
			{
				if (!(_position < _parts.Length))
					return string.Empty;

				return _parts[_position];
			}

			string GetNextPart()
			{
				_position++;
				return GetPart();
			}

			void CreateLinearGradient(double angle)
			{
				var coordinates = GetCoordinatesByAngle(angle);
				var startPoint = coordinates.Item1;
				var endPoint = coordinates.Item2;

				_gradient = new LinearGradientBrush
				{
					StartPoint = startPoint,
					EndPoint = endPoint,
					GradientStops = new GradientStopCollection()
				};
			}

			void CreateRadialGradient(Point center)
			{
				_gradient = new RadialGradientBrush
				{
					Center = center,
					GradientStops = new GradientStopCollection()
				};
			}

			void AddGradientStop(Color color, float? offset = null)
			{
				if (_gradient == null)
				{
					CreateLinearGradient(0);
				}

				var gradientStop = new GradientStop
				{
					Color = color,
					Offset = offset ?? -1
				};

				_gradient.GradientStops.Add(gradientStop);
			}

			void AddGradientStops(Color color, IEnumerable<float> offsets)
			{
				foreach (var offset in offsets)
					AddGradientStop(color, offset);
			}

			Tuple<Point, Point> GetCoordinatesByAngle(double angle)
			{
				Point startPoint;
				Point endPoint;

				switch (angle)
				{
					case 90:
						startPoint = new Point(0, 1);
						endPoint = new Point(0, 0);
						break;
					case 180:
						startPoint = new Point(1, 0);
						endPoint = new Point(0, 0);
						break;
					case 270:
						startPoint = new Point(0, 0);
						endPoint = new Point(0, 1);
						break;
					default:
					case 360:
						startPoint = new Point(0, 0);
						endPoint = new Point(1, 0);
						break;
				}

				return new Tuple<Point, Point>(startPoint, endPoint);
			}

			bool TryParseAngle(string part, out double angle)
			{
				if (TryParseNumber(part, "deg", out var degrees))
				{
					angle = degrees % 360;
					return true;
				}

				if (TryParseNumber(part, "turn", out var turn))
				{
					angle = 360 * turn;
					return true;
				}

				angle = 0;
				return false;
			}

			Point GetGradientCenter()
			{
				_position++;

				var part = GetPart().Trim();

				int gradientCenterPosition = 1;
				var parts = part.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts.Length > gradientCenterPosition)
				{
					if (parts[gradientCenterPosition].IndexOf("at", StringComparison.Ordinal) != -1)
					{
						gradientCenterPosition++;
						var directionX = gradientCenterPosition < parts.Length ? parts[gradientCenterPosition].Trim() : string.Empty;

						gradientCenterPosition++;
						var directionY = gradientCenterPosition < parts.Length ? parts[gradientCenterPosition].Trim() : string.Empty;

						var hasPositionX = TryParseOffset(directionX, out var positionX);
						var hasPositionY = TryParseOffset(directionY, out var positionY);

						var position = new Point(0.5, 0.5);

						if (!hasPositionX && !string.IsNullOrEmpty(directionX))
							position = GetGradientPositionByDirection(directionX);

						if (!hasPositionY && !string.IsNullOrEmpty(directionY))
							position = GetGradientPositionByDirection(directionY);

						return new Point(hasPositionX ? positionX : position.X, hasPositionY ? positionY : position.Y);
					}
				}

				return new Point(0.5, 0.5);
			}

			Point GetGradientPositionByDirection(string direction)
			{
				switch (direction)
				{
					case "left":
						return new Point(0, 0.5);
					case "right":
						return new Point(1, 0.5);
					case "top":
						return new Point(0.5, 0);
					case "bottom":
						return new Point(0.5, 1);
					default:
					case "center":
						return new Point(0.5, 0.5);
				}
			}

			bool TryParseNumber(string part, string unit, out float result)
			{
				if (part.EndsWith(unit))
				{
					var index = part.LastIndexOf(unit, StringComparison.OrdinalIgnoreCase);
					var number = part.Substring(0, index);

					if (float.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
					{
						result = value;
						return true;
					}
				}

				result = 0;
				return false;
			}

			bool TryParseOffset(string part, out float result)
			{
				if (part != null)
				{
					// Using percentage
					if (TryParseNumber(part, "%", out var value))
					{
						result = Math.Min(value / 100, 1f);
						return true;
					}

					// Using px
					if (TryParseNumber(part, "px", out result))
					{
						return true;
					}
				}

				result = 0;
				return false;
			}

			bool TryParseOffsets(string[] parts, out float[] result)
			{
				var offsets = new List<float>();

				foreach (var part in parts)
				{
					if (TryParseOffset(part, out var offset))
						offsets.Add(offset);
				}

				result = offsets.ToArray();
				return result.Length > 0;
			}
		}
	}
}
