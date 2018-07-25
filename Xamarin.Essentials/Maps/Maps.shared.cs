using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Maps
    {
        public static Task OpenAsync(Location location) =>
            OpenAsync(location, new MapsLaunchOptions());

        public static Task OpenAsync(Location location, MapsLaunchOptions options)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return PlatformOpenMapsAsync(location.Latitude, location.Longitude, options);
        }

        public static Task OpenAsync(double latitude, double longitude) =>
            OpenAsync(latitude, longitude, new MapsLaunchOptions());

        public static Task OpenAsync(double latitude, double longitude, MapsLaunchOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return PlatformOpenMapsAsync(latitude, longitude, options);
        }

        public static Task OpenAsync(Placemark placemark) =>
            OpenAsync(placemark, new MapsLaunchOptions());

        public static Task OpenAsync(Placemark placemark, MapsLaunchOptions options)
        {
            if (placemark == null)
                throw new ArgumentNullException(nameof(placemark));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return PlatformOpenMapsAsync(placemark, options);
        }
    }
}
