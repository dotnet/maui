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
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <inheritdoc/>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString()?.Trim();

			if (string.IsNullOrEmpty(strValue))
				throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Location)}");

			var parts = strValue.Split(',');

			if (parts.Length == 2
				&& double.TryParse(parts[0].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double latitude)
				&& double.TryParse(parts[1].Trim(), NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double longitude))
			{
				return new Location(latitude, longitude);
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Location)}");
		}

		/// <inheritdoc/>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not Location location)
				throw new NotSupportedException();

			return $"{location.Latitude.ToString(CultureInfo.InvariantCulture)},{location.Longitude.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
