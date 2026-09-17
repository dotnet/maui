using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Type converter for converting a properly formatted string to a <see cref="Shadow"/>.
	/// </summary>
	public class ShadowTypeConverter : TypeConverter
	{
		const int MaxShadowParts = 5;
		const int ValuesPerPart = 2;

		readonly ColorTypeConverter _colorTypeConverter = new ColorTypeConverter();

		/// <summary>
		/// Checks whether the given <paramref name="sourceType" /> is a string.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="sourceType">The type to convert from.</param>
		/// <returns></returns>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
			=> sourceType == typeof(string);

		/// <summary>
		/// Checks whether the given <paramref name="destinationType" /> is a Shadow.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="destinationType">The type to convert to.</param>
		/// <returns></returns>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(Shadow);

		/// <summary>
		/// Converts <paramref name="value" /> to a Shadow.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="culture">The culture to use for conversion.</param>
		/// <param name="value">The value to convert.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="value" /> is not a valid Shadow.</exception>
		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue is null)
			{
				throw new ArgumentNullException(nameof(strValue));
			}

			try
			{
				Span<int> parts = stackalloc int[MaxShadowParts * ValuesPerPart];
				var partCount = Tokenize(strValue, parts);

				if (partCount == 3) // <color> | <float> | <float> e.g. #000000 4 4
				{
					var brush = ParseBrush(GetPartString(strValue, parts, 0));
					var offsetX = float.Parse(GetPartString(strValue, parts, 1), CultureInfo.InvariantCulture);
					var offsetY = float.Parse(GetPartString(strValue, parts, 2), CultureInfo.InvariantCulture);

					return new Shadow
					{
						Brush = brush,
						Offset = new Point(offsetX, offsetY)
					};
				}
				else if (partCount == 4) // <float> | <float> | <float> | <color> e.g. 4 4 16 #000000
				{
					var offsetX = float.Parse(GetPartString(strValue, parts, 0), CultureInfo.InvariantCulture);
					var offsetY = float.Parse(GetPartString(strValue, parts, 1), CultureInfo.InvariantCulture);
					var radius = float.Parse(GetPartString(strValue, parts, 2), CultureInfo.InvariantCulture);
					var brush = ParseBrush(GetPartString(strValue, parts, 3));

					return new Shadow
					{
						Offset = new Point(offsetX, offsetY),
						Radius = radius,
						Brush = brush
					};
				}
				else if (partCount == 5) // <float> | <float> | <float> | <color> | <float> e.g. 4 4 16 #000000 0.5
				{
					var offsetX = float.Parse(GetPartString(strValue, parts, 0), CultureInfo.InvariantCulture);
					var offsetY = float.Parse(GetPartString(strValue, parts, 1), CultureInfo.InvariantCulture);
					var radius = float.Parse(GetPartString(strValue, parts, 2), CultureInfo.InvariantCulture);
					var brush = ParseBrush(GetPartString(strValue, parts, 3));
					var opacity = float.Parse(GetPartString(strValue, parts, 4), CultureInfo.InvariantCulture);

					return new Shadow
					{
						Offset = new Point(offsetX, offsetY),
						Radius = radius,
						Brush = brush,
						Opacity = opacity
					};
				}
			}
			catch (FormatException ex)
			{
				throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Shadow)}.", ex);
			}
			catch (OverflowException ex)
			{
				throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Shadow)}.", ex);
			}
			catch (InvalidOperationException ex)
			{
				throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Shadow)}.", ex);
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(IShadow)}.");
		}

		static string GetPartString(string value, ReadOnlySpan<int> parts, int partIndex)
		{
			var offset = partIndex * ValuesPerPart;
			return value.Substring(parts[offset], parts[offset + 1]);
		}

		static int Tokenize(string value, Span<int> parts)
		{
			var input = value.AsSpan();
			var partCount = 0;
			var position = 0;

			while (position < input.Length)
			{
				var remaining = input.Slice(position);
				if (!TryReadColor(remaining, out var length) &&
					!TryReadNumber(remaining, out length))
				{
					position++;
					continue;
				}

				if (partCount < MaxShadowParts)
				{
					var offset = partCount * ValuesPerPart;
					parts[offset] = position;
					parts[offset + 1] = length;
				}

				partCount++;
				if (partCount > MaxShadowParts)
				{
					break;
				}

				position += length;
			}

			return partCount;
		}

		static bool TryReadColor(ReadOnlySpan<char> value, out int length)
		{
			if (TryReadHexColor(value, out length) ||
				TryReadRgbColor(value, "rgb(", hasAlpha: false, percentages: true, out length) ||
				TryReadRgbColor(value, "rgba(", hasAlpha: true, percentages: true, out length) ||
				TryReadRgbColor(value, "rgb(", hasAlpha: false, percentages: false, out length) ||
				TryReadRgbColor(value, "rgba(", hasAlpha: true, percentages: false, out length) ||
				TryReadHsxColor(value, "hsl(", hasAlpha: false, out length) ||
				TryReadHsxColor(value, "hsla(", hasAlpha: true, out length) ||
				TryReadHsxColor(value, "hsv(", hasAlpha: false, out length) ||
				TryReadHsxColor(value, "hsva(", hasAlpha: true, out length))
			{
				return true;
			}

			var position = 0;
			while (position < value.Length && IsAsciiLetter(value[position]))
			{
				position++;
			}

			length = position;
			return position > 0;
		}

		static bool TryReadHexColor(ReadOnlySpan<char> value, out int length)
		{
			if (value.IsEmpty || value[0] != '#')
			{
				length = 0;
				return false;
			}

			var position = 1;
			while (position < value.Length && position <= 8 && IsHexDigit(value[position]))
			{
				position++;
			}

			length = position;
			return position >= 4;
		}

		static bool TryReadRgbColor(ReadOnlySpan<char> value, ReadOnlySpan<char> prefix, bool hasAlpha, bool percentages, out int length)
		{
			if (!value.StartsWith(prefix, StringComparison.Ordinal))
			{
				length = 0;
				return false;
			}

			var position = prefix.Length;
			SkipWhitespace(value, ref position);

			for (var component = 0; component < 3; component++)
			{
				if (!TryReadDigits(value, ref position) ||
					(percentages && !TryReadCharacter(value, ref position, '%')))
				{
					length = 0;
					return false;
				}

				SkipWhitespace(value, ref position);
				if (component < 2 && !TryReadSeparator(value, ref position))
				{
					length = 0;
					return false;
				}
			}

			if (hasAlpha)
			{
				if (!TryReadSeparator(value, ref position) ||
					!TryReadDecimal(value, ref position))
				{
					length = 0;
					return false;
				}

				SkipWhitespace(value, ref position);
			}

			if (!TryReadCharacter(value, ref position, ')'))
			{
				length = 0;
				return false;
			}

			length = position;
			return true;
		}

		static bool TryReadHsxColor(ReadOnlySpan<char> value, ReadOnlySpan<char> prefix, bool hasAlpha, out int length)
		{
			if (!value.StartsWith(prefix, StringComparison.Ordinal))
			{
				length = 0;
				return false;
			}

			var position = prefix.Length;
			SkipWhitespace(value, ref position);

			if (!TryReadDigits(value, ref position))
			{
				length = 0;
				return false;
			}

			SkipWhitespace(value, ref position);
			for (var component = 0; component < 2; component++)
			{
				if (!TryReadSeparator(value, ref position) ||
					!TryReadDigits(value, ref position) ||
					!TryReadCharacter(value, ref position, '%'))
				{
					length = 0;
					return false;
				}

				SkipWhitespace(value, ref position);
			}

			if (hasAlpha)
			{
				if (!TryReadSeparator(value, ref position) ||
					!TryReadDecimal(value, ref position))
				{
					length = 0;
					return false;
				}

				SkipWhitespace(value, ref position);
			}

			if (!TryReadCharacter(value, ref position, ')'))
			{
				length = 0;
				return false;
			}

			length = position;
			return true;
		}

		static bool TryReadSeparator(ReadOnlySpan<char> value, ref int position)
		{
			if (!TryReadCharacter(value, ref position, ','))
			{
				return false;
			}

			SkipWhitespace(value, ref position);
			return true;
		}

		static bool TryReadDecimal(ReadOnlySpan<char> value, ref int position)
		{
			if (!TryReadDigits(value, ref position))
			{
				return false;
			}

			if (position < value.Length && value[position] == '.')
			{
				var decimalPoint = position++;
				if (!TryReadDigits(value, ref position))
				{
					position = decimalPoint;
				}
			}

			return true;
		}

		static bool TryReadNumber(ReadOnlySpan<char> value, out int length)
		{
			var position = 0;
			if (position < value.Length && value[position] == '-')
			{
				position++;
			}

			if (!TryReadDigits(value, ref position))
			{
				length = 0;
				return false;
			}

			if (position < value.Length && value[position] == '.')
			{
				var decimalPoint = position++;
				if (!TryReadDigits(value, ref position))
				{
					position = decimalPoint;
				}
			}

			if (position < value.Length && (value[position] == 'e' || value[position] == 'E'))
			{
				var exponent = position++;
				if (position < value.Length && (value[position] == '+' || value[position] == '-'))
				{
					position++;
				}

				if (!TryReadDigits(value, ref position))
				{
					position = exponent;
				}
			}

			length = position;
			return true;
		}

		static bool TryReadDigits(ReadOnlySpan<char> value, ref int position)
		{
			var start = position;
			while (position < value.Length && char.IsDigit(value[position]))
			{
				position++;
			}

			return position > start;
		}

		static bool TryReadCharacter(ReadOnlySpan<char> value, ref int position, char expected)
		{
			if (position >= value.Length || value[position] != expected)
			{
				return false;
			}

			position++;
			return true;
		}

		static void SkipWhitespace(ReadOnlySpan<char> value, ref int position)
		{
			while (position < value.Length && char.IsWhiteSpace(value[position]))
			{
				position++;
			}
		}

		static bool IsAsciiLetter(char value)
			=> value is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

		static bool IsHexDigit(char value)
			=> value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

		/// <summary>
		/// Converts a Shadow to a string.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="culture">The culture to use for conversion.</param>
		/// <param name="value">The Shadow to convert.</param>
		/// <param name="destinationType">The type to convert to.</param>
		/// <returns>A string representation of the Shadow.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="value" /> is not a Shadow or the Brush is not a SolidColorBrush.</exception>
		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
		{
			if (value is null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (value is Shadow shadow)
			{
				var offsetX = shadow.Offset.X.ToString(CultureInfo.InvariantCulture);
				var offsetY = shadow.Offset.Y.ToString(CultureInfo.InvariantCulture);
				var radius = shadow.Radius.ToString(CultureInfo.InvariantCulture);
				var color = (shadow.Brush as SolidColorBrush)?.Color.ToHex();
				var opacity = shadow.Opacity.ToString(CultureInfo.InvariantCulture);

				if (color is null)
				{
					throw new InvalidOperationException("Cannot convert Shadow to string: Brush is not a valid SolidColorBrush or has no Color.");
				}

				return $"{offsetX} {offsetY} {radius} {color} {opacity}";
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into string.");
		}

		/// <summary>
		/// Parses a string value into a SolidColorBrush.
		/// </summary>
		/// <param name="value">The value to parse.</param>
		/// <returns>A SolidColorBrush.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the value is not a SolidColorBrush or has no Color.</exception>
		SolidColorBrush ParseBrush(string value)
		{
			// If the value is a color, return a SolidColorBrush
			if (_colorTypeConverter.ConvertFrom(value) is Color color)
			{
				return new SolidColorBrush(color);
			}

			throw new InvalidOperationException("Cannot convert Shadow to string: Brush is not a valid SolidColorBrush or has no Color.");
		}
	}
}
