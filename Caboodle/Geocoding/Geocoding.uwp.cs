using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Microsoft.Caboodle
{
	public partial class Geocoding
    {
        public static async Task<IEnumerable<Location>> GetLocationsAsync(double latitude, double longitude)
        {

            var queryResults =
                await MapLocationFinder.FindLocationsAtAsync(
                        new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude })).AsTask();

            return queryResults?.Locations.ToAddresses();
        }

        public static async Task<IEnumerable<GeoPoint>> GetGeoPointsAsync(string address)
        {
            SetMapKey();

            var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);

            if (queryResults?.Locations == null)
                return null;

            return queryResults.Locations.Select(l  => new GeoPoint
			{
				Latitude = l.Point.Position.Latitude,
				Longitude = l.Point.Position.Longitude
			});
        }

        static void SetMapKey()
        {
            if (string.IsNullOrWhiteSpace(MapKey) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
            {
                Console.WriteLine("Map API key is required on UWP to reverse geolocate.");
                throw new ArgumentNullException(nameof(MapKey));

            }

            if (!string.IsNullOrWhiteSpace(MapKey))
                MapService.ServiceToken = MapKey;
        }
    }
}
