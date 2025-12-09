#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Microsoft.Maui.Devices.Sensors
{
	class GeocodingImplementation : IPlatformGeocoding, IGeocoding
	{
		public string? MapServiceToken { get; set; }

		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
			ValidateMapServiceToken();

			var point = new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude });

			var queryResults = await MapLocationFinder.FindLocationsAtAsync(point).AsTask();

			return queryResults?.Locations?.ToPlacemarks() ?? Array.Empty<Placemark>();
		}

		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
		{
			ValidateMapServiceToken();

			var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);

			return queryResults?.Locations?.ToLocations() ?? Array.Empty<Location>();
		}

		void ValidateMapServiceToken()
		{
			if (string.IsNullOrWhiteSpace(MapServiceToken) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
				throw new ArgumentNullException(nameof(MapServiceToken), "Please set the map service token to be able to use this API.");

			if (!string.IsNullOrWhiteSpace(MapServiceToken))
				MapService.ServiceToken = MapServiceToken;
		}
	}
}
