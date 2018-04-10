using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Xamarin.Essentials
{
    public static partial class Geocoding
    {
        public static async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
        {
            ValidateMapKey();

            var point = new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude });

            var queryResults = await MapLocationFinder.FindLocationsAtAsync(point).AsTask();

            return queryResults?.Locations?.ToPlacemarks();
        }

        public static async Task<IEnumerable<Location>> GetLocationsAsync(string address)
        {
            ValidateMapKey();

            var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);

            return queryResults?.Locations?.ToLocations();
        }

        internal static void ValidateMapKey()
        {
            if (string.IsNullOrWhiteSpace(MapKey) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
                throw new ArgumentNullException(nameof(MapKey));

            if (!string.IsNullOrWhiteSpace(MapKey))
                MapService.ServiceToken = MapKey;
        }
    }
}
