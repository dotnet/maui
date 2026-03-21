using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Type converter for converting a properly formatted string to a <see cref="Shadow"/>.
	/// </summary>
	public class ShadowTypeConverter : TypeConverter
	{
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
				var regex = new Regex(@"
                    # Match colors
                    (
                        \#([0-9a-fA-F]{3,8}) # Hex colors (#RGB, #RRGGBB, #RRGGBBAA)
                        |rgb\(\s*\d+%\s*,\s*\d+%\s*,\s*\d+%\s*\) # rgb(percent, percent, percent)
                        |rgba\(\s*\d+%\s*,\s*\d+%\s*,\s*\d+%\s*,\s*\d+(?:\.\d+)?\s*\) # rgba(percent, percent, percent, alpha)
                        |rgb\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*\) # rgb(int, int, int)
                        |rgba\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*,\s*\d+(?:\.\d+)?\s*\) # rgba(int, int, int, alpha)
                        |hsl\(\s*\d+\s*,\s*\d+%\s*,\s*\d+%\s*\) # hsl(hue, saturation, lightness)
                        |hsla\(\s*\d+\s*,\s*\d+%\s*,\s*\d+%\s*,\s*\d+(?:\.\d+)?\s*\) # hsla(hue, saturation, lightness, alpha)
                        |hsv\(\s*\d+\s*,\s*\d+%\s*,\s*\d+%\s*\) # hsl(hue, saturation, value)
                        |hsva\(\s*\d+\s*,\s*\d+%\s*,\s*\d+%\s*,\s*\d+(?:\.\d+)?\s*\) # hsla(hue, saturation, value, alpha)
                        |[a-zA-Z]+ # X11 named colors (e.g., AliceBlue, limegreen)
    
                    )
                    | # Match numbers
                    (
                        -?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?  # Floats or scientific notation
                    )
                ", RegexOptions.IgnorePatternWhitespace);

				var matches = regex.Matches(strValue);
				//var parts = matches.Select(m => m.Value).ToArray();

				if (matches.Count == 3) // <color> | <float> | <float> e.g. #000000 4 4
				{
					var brush = ParseBrush(matches[0].Value);
					var offsetX = float.Parse(matches[1].Value, CultureInfo.InvariantCulture);
					var offsetY = float.Parse(matches[2].Value, CultureInfo.InvariantCulture);

					return new Shadow
					{
						Brush = brush,
						Offset = new Point(offsetX, offsetY)
					};
				}
				else if (matches.Count == 4) // <float> | <float> | <float> | <color> e.g. 4 4 16 #000000
				{
					var offsetX = float.Parse(matches[0].Value, CultureInfo.InvariantCulture);
					var offsetY = float.Parse(matches[1].Value, CultureInfo.InvariantCulture);
					var radius = float.Parse(matches[2].Value, CultureInfo.InvariantCulture);
					var brush = ParseBrush(matches[3].Value);

					return new Shadow
					{
						Offset = new Point(offsetX, offsetY),
						Radius = radius,
						Brush = brush
					};
				}
				else if (matches.Count == 5) // <float> | <float> | <float> | <color> | <float> e.g. 4 4 16 #000000 0.5
				{
					var offsetX = float.Parse(matches[0].Value, CultureInfo.InvariantCulture);
					var offsetY = float.Parse(matches[1].Value, CultureInfo.InvariantCulture);
					var radius = float.Parse(matches[2].Value, CultureInfo.InvariantCulture);
					var brush = ParseBrush(matches[3].Value);
					var opacity = float.Parse(matches[4].Value, CultureInfo.InvariantCulture);

					return new Shadow
					{
						Offset = new Point(offsetX, offsetY),
						Radius = radius,
						Brush = brush,
						Opacity = opacity
					};
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Shadow)}.", ex);
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(IShadow)}.");
		}

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
