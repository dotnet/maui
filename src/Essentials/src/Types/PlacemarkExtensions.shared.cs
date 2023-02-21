using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// This class contains static extension methods for use with <see cref="Placemark"/> objects.
	/// </summary>
	public static partial class PlacemarkExtensions
	{
		/// <inheritdoc cref="Map.OpenAsync(Placemark, MapLaunchOptions)"/>
		public static Task OpenMapsAsync(this Placemark placemark, MapLaunchOptions options) =>
			Map.OpenAsync(placemark, options);

		/// <inheritdoc cref="Map.OpenAsync(Placemark)"/>
		public static Task OpenMapsAsync(this Placemark placemark) =>
			Map.OpenAsync(placemark);

		internal static string GetEscapedAddress(this Placemark placemark)
		{
			if (placemark == null)
				throw new ArgumentNullException(nameof(placemark));

			var address = $"{placemark.Thoroughfare} {placemark.Locality} {placemark.AdminArea} {placemark.PostalCode} {placemark.CountryName}";

			return Uri.EscapeDataString(address);
		}
	}
}
