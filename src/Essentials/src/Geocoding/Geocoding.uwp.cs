using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class GeocodingImplementation: IGeocoding
	{
		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
			ValidateMapServiceToken();

			var point = new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude });

			var queryResults = await MapLocationFinder.FindLocationsAtAsync(point).AsTask();

			return queryResults?.Locations?.ToPlacemarks();
		}

		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
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
