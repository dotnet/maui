#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices.Sensors
{
	public interface IGeocoding
	{
		Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude);

		Task<IEnumerable<Location>> GetLocationsAsync(string address);
	}

	public interface IPlatformGeocoding : IGeocoding
	{
#if WINDOWS || TIZEN
		string? MapServiceToken { get; set; }
#endif
	}

	public static class Geocoding
	{
		static IGeocoding? defaultImplementation;

		public static IGeocoding Default =>
			defaultImplementation ??= new GeocodingImplementation();

		internal static void SetCurrent(IGeocoding? implementation) =>
			defaultImplementation = implementation;
	}

	public static class GeocodingExtensions
	{
		public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(this IGeocoding geocoding, Location location)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			return geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
		}

#if WINDOWS || TIZEN
		public static string? GetMapServiceToken(this IGeocoding geocoding)
		{
			if (geocoding is not IPlatformGeocoding platform)
				throw new PlatformNotSupportedException("This implementation of IGeocoding does not implement IPlatformGeocoding.");

			return platform.MapServiceToken;
		}
		public static void SetMapServiceToken(this IGeocoding geocoding, string? mapServiceToken)
		{
			if (geocoding is not IPlatformGeocoding platform)
				throw new PlatformNotSupportedException("This implementation of IGeocoding does not implement IPlatformGeocoding.");

			platform.MapServiceToken = mapServiceToken;
		}
#endif
	}
}
