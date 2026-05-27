#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// A type converter that converts a string representation of latitude and longitude to a <see cref="Location"/> object.
	/// </summary>
	/// <remarks>
	/// Supported formats:
	/// <list type="bullet">
	/// <item><description><c>"latitude,longitude"</c> (e.g., <c>"36.9628066,-122.0194722"</c>)</description></item>
	/// </list>
	/// </remarks>
	public class LocationTypeConverter : TypeConverter
	{
		/// <summary>Determines whether conversion is possible from the specified type to <see cref="Location"/>.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="sourceType">The source type to check.</param>
		/// <returns><see langword="true"/> if the source type is <see cref="string"/>; otherwise, <see langword="false"/>.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		/// <summary>Determines whether conversion is possible from <see cref="Location"/> to the specified type.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="destinationType">The destination type to check.</param>
		/// <returns><see langword="true"/> if the destination type is <see cref="string"/>; otherwise, <see langword="false"/>.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <summary>Converts a string representation of latitude and longitude to a <see cref="Location"/> object.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="culture">The culture info.</param>
		/// <param name="value">The value to convert, expected in the format <c>"latitude,longitude"</c>.</param>
		/// <returns>A <see cref="Location"/> object parsed from the string value.</returns>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString()?.Trim();

			if (string.IsNullOrEmpty(strValue))
				throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Location)}");

			var parts = strValue!.Split(',');

			if (parts.Length == 2
				&& double.TryParse(parts[0].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double latitude)
				&& double.TryParse(parts[1].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double longitude))
			{
				return new Location(latitude, longitude);
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Location)}");
		}

		/// <summary>Converts a <see cref="Location"/> object to a string representation.</summary>
		/// <param name="context">The format context.</param>
		/// <param name="culture">The culture info.</param>
		/// <param name="value">The <see cref="Location"/> value to convert.</param>
		/// <param name="destinationType">The destination type (must be <see cref="string"/>).</param>
		/// <returns>A string in the format <c>"latitude,longitude"</c>.</returns>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not Location location)
				throw new NotSupportedException();

			return $"{location.Latitude.ToString(CultureInfo.InvariantCulture)},{location.Longitude.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
