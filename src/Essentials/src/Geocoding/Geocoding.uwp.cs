using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Microsoft.Maui.Essentials
{
	public static partial class Geocoding
	{
		static async Task<IEnumerable<Placemark>> PlatformGetPlacemarksAsync(double latitude, double longitude)
		{
			ValidateMapServiceToken();

			var point = new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude });

			var queryResults = await MapLocationFinder.FindLocationsAtAsync(point).AsTask();

			return queryResults?.Locations?.ToPlacemarks();
		}

		static async Task<IEnumerable<Location>> PlatformGetLocationsAsync(string address)
		{
			ValidateMapServiceToken();

			var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);

			return queryResults?.Locations?.ToLocations();
		}

		internal static void ValidateMapServiceToken()
		{
			if (string.IsNullOrWhiteSpace(Platform.MapServiceToken) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
				throw new ArgumentNullException(nameof(Platform.MapServiceToken), "Please set the map service token(MapService.ServiceToken) to be able to use this API.");

			if (!string.IsNullOrWhiteSpace(Platform.MapServiceToken))
				MapService.ServiceToken = Platform.MapServiceToken;
		}
	}
}
