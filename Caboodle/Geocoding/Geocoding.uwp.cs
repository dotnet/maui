using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Microsoft.Caboodle
{
	public partial class Geocoding
    {
        public static async Task<IEnumerable<Address>> GetAddressesAsync(double latitude, double longitude)
        {

            var queryResults =
                await MapLocationFinder.FindLocationsAtAsync(
                        new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude })).AsTask();

            return queryResults?.Locations.ToAddresses();
        }

        public static async Task<IEnumerable<Position>> GetPositionsAsync(string address)
        {
            SetMapKey();

            var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);
            var positions = new List<Position>();
            if (queryResults?.Locations == null)
                return positions;

            foreach (var p in queryResults.Locations)
            {
                positions.Add(new Position
                {
                    Latitude = p.Point.Position.Latitude,
                    Longitude = p.Point.Position.Longitude
                });
            }

            return positions;
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
