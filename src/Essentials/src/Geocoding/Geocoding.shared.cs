#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// The Geocoding API provides functionality to geocode a placemark to positional coordinates and reverse-geocode coordinates to a placemark.
	/// </summary>
	public interface IGeocoding
	{
		/// <summary>
		/// Retrieve potential placemarks for a given location specified by coordinates.
		/// </summary>
		/// <param name="latitude">The latitude coordinate to find placemarks near.</param>
		/// <param name="longitude">The longitude coordinate to find placemarks near.</param>
		/// <returns>List of <see cref="Placemark"/> that best match the coordinates or <see langword="null"/> if no placemarks are found.</returns>
		Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude);

		/// <summary>
		/// Retrieve potential locations for a given address.
		/// </summary>
		/// <param name="address">Address to retrieve the location for.</param>
		/// <returns>List of <see cref="Location"/> that best match the address or <see langword="null"/> if no locations are found.</returns>
		Task<IEnumerable<Location>> GetLocationsAsync(string address);
	}

	/// <summary>
	/// Separate abstraction for platform-specific geocoding APIs.
	/// </summary>
	public interface IPlatformGeocoding : IGeocoding
	{
#if WINDOWS || TIZEN
		/// <summary>
		/// Gets or sets the map service API key for this platform.
		/// </summary>
		/// <remarks>Only needed for Windows and Tizen.</remarks>
		string? MapServiceToken { get; set; }
#endif
	}

	/// <summary>
	/// The Geocoding API provides functionality to geocode a placemark to positional coordinates and reverse-geocode coordinates to a placemark.
	/// </summary>
	public static class Geocoding
	{
		/// <summary>
		/// Retrieve potential placemarks for a given location specified by <see cref="Location"/>.
		/// </summary>
		/// <param name="location">A <see cref="Location"/> instance to find placemarks near.</param>
		/// <returns>List of <see cref="Placemark"/> that best match the coordinates or <see langword="null"/> if no placemarks are found.</returns>
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(Location location) =>
			Current.GetPlacemarksAsync(location);

		/// <summary>
		/// Retrieve potential placemarks for a given location specified by coordinates.
		/// </summary>
		/// <param name="latitude">The latitude coordinate to find placemarks.</param>
		/// <param name="longitude">The longitude coordinate to find placemarks.</param>
		/// <returns>List of <see cref="Placemark"/> that best match the coordinates or <see langword="null"/> if no placemarks are found.</returns>
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
			Current.GetPlacemarksAsync(latitude, longitude);

		/// <summary>
		/// Retrieve potential locations for a given address.
		/// </summary>
		/// <param name="address">Address to retrieve the location for.</param>
		/// <returns>List of <see cref="Location"/> that best match the address or <see langword="null"/> if no locations are found.</returns>
		public static Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
			Current.GetLocationsAsync(address);

		static IGeocoding Current => Devices.Sensors.Geocoding.Default;

		static IGeocoding? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IGeocoding Default =>
			defaultImplementation ??= new GeocodingImplementation();

		internal static void SetCurrent(IGeocoding? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Static class with extension methods for the <see cref="IGeocoding"/> APIs.
	/// </summary>
	public static class GeocodingExtensions
	{
		/// <summary>
		/// Retrieve potential placemarks for a given location specified by <see cref="Location"/>.
		/// </summary>
		/// <param name="geocoding">The object this method is invoked on.</param>
		/// <param name="location">A <see cref="Location"/> instance to find placemarks near.</param>
		/// <returns>List of <see cref="Placemark"/> that best match the coordinates or <see langword="null"/> if no placemarks are found.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="location"/> is <see langword="null"/>.</exception>
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(this IGeocoding geocoding, Location location)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			return geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
		}

#if WINDOWS || TIZEN
		/// <summary>
		/// Gets the map service API key for this platform.
		/// </summary>
		/// <param name="geocoding">The object this method is invoked on.</param>
		/// <remarks>Only needed for Windows and Tizen.</remarks>
		/// <returns>The currently configured map service API key, or <see langword="null"/> if it's not set.</returns>
		/// <exception cref="PlatformNotSupportedException">
		/// Thrown when the current platform does not require any API key for geocoding.
		/// This is checked by confirming that <see cref="IGeocoding"/> implements the <see cref="IPlatformGeocoding"/> interface.
		/// </exception>
		public static string? GetMapServiceToken(this IGeocoding geocoding)
		{
			if (geocoding is not IPlatformGeocoding platform)
				throw new PlatformNotSupportedException("This implementation of IGeocoding does not implement IPlatformGeocoding.");

			return platform.MapServiceToken;
		}

		/// <summary>
		/// Sets the map service API key for this platform.
		/// </summary>
		/// <param name="geocoding">The object this method is invoked on.</param>
		/// <param name="mapServiceToken">The map service API key.</param>
		/// <remarks>Only needed for Windows and Tizen.</remarks>
		/// <exception cref="PlatformNotSupportedException">
		/// Thrown when the current platform does not require any API key for geocoding.
		/// This is checked by confirming that <see cref="IGeocoding"/> implements the <see cref="IPlatformGeocoding"/> interface.
		/// </exception>
		public static void SetMapServiceToken(this IGeocoding geocoding, string? mapServiceToken)
		{
			if (geocoding is not IPlatformGeocoding platform)
				throw new PlatformNotSupportedException("This implementation of IGeocoding does not implement IPlatformGeocoding.");

			platform.MapServiceToken = mapServiceToken;
		}
#endif
	}
}
