#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// A type converter that converts a string representation to a <see cref="MapSpan"/> object.
	/// </summary>
	/// <remarks>
	/// Supported formats:
	/// <list type="bullet">
	/// <item><description><c>"latitude,longitude,latitudeDegrees,longitudeDegrees"</c> (e.g., <c>"36.9628,-122.0195,0.01,0.01"</c>)</description></item>
	/// </list>
	/// </remarks>
	public class MapSpanTypeConverter : TypeConverter
	{
		/// <summary>Determines whether conversion is possible from the specified type to <see cref="MapSpan"/>.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="sourceType">The source type to check.</param>
		/// <returns><see langword="true"/> if the source type is <see cref="string"/>; otherwise, <see langword="false"/>.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		/// <summary>Determines whether conversion is possible from <see cref="MapSpan"/> to the specified type.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="destinationType">The destination type to check.</param>
		/// <returns><see langword="true"/> if the destination type is <see cref="string"/>; otherwise, <see langword="false"/>.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <summary>Converts a string representation to a <see cref="MapSpan"/> object.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="culture">The culture info.</param>
		/// <param name="value">The value to convert, expected in the format <c>"latitude,longitude,latitudeDegrees,longitudeDegrees"</c>.</param>
		/// <returns>A <see cref="MapSpan"/> object parsed from the string value.</returns>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString()?.Trim();

			if (string.IsNullOrEmpty(strValue))
				throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(MapSpan)}");

			var parts = strValue!.Split(',');

			if (parts.Length == 4
				&& double.TryParse(parts[0].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double latitude)
				&& double.TryParse(parts[1].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double longitude)
				&& double.TryParse(parts[2].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double latDegrees)
				&& double.TryParse(parts[3].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double lonDegrees))
			{
				return new MapSpan(new Location(latitude, longitude), latDegrees, lonDegrees);
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(MapSpan)}");
		}

		/// <summary>Converts a <see cref="MapSpan"/> object to a string representation.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="culture">The culture info.</param>
		/// <param name="value">The <see cref="MapSpan"/> value to convert.</param>
		/// <param name="destinationType">The destination type (must be <see cref="string"/>).</param>
		/// <returns>A string in the format <c>"latitude,longitude,latitudeDegrees,longitudeDegrees"</c>.</returns>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not MapSpan span)
				throw new NotSupportedException();

			return $"{span.Center.Latitude.ToString(CultureInfo.InvariantCulture)},{span.Center.Longitude.ToString(CultureInfo.InvariantCulture)}," +
				$"{span.LatitudeDegrees.ToString(CultureInfo.InvariantCulture)},{span.LongitudeDegrees.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
