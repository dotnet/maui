using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Geocoding
    {
        public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(Location location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return GetPlacemarksAsync(location.Latitude, location.Longitude);
        }

        public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
            => PlatformGetPlacemarksAsync(latitude, longitude);

        public static Task<IEnumerable<Location>> GetLocationsAsync(string address)
            => PlatformGetLocationsAsync(address);
    }
}
