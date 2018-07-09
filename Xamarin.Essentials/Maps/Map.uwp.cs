using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(Location location, MapLaunchOptions options)
            => PlatformOpenMapsAsync(location.Latitude, location.Longitude, options);

        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = CreatePointUri(latitude, longitude, ref options);
            return LaunchUri(uri);
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            var uri = CreatePlacemarkUri(placemark, ref options);
            return LaunchUri(uri);
        }

        static async Task LaunchUri(Uri mapsUri)
        {// Use launcher api once it is available
            await Windows.System.Launcher.LaunchUriAsync(mapsUri);
        }

        static Uri CreatePlacemarkUri(Placemark placemark, ref MapLaunchOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Name))
                options.Name = string.Empty;

            if (string.IsNullOrWhiteSpace(placemark.Thoroughfare))
                placemark.Thoroughfare = string.Empty;

            if (string.IsNullOrWhiteSpace(placemark.Locality))
                placemark.Locality = string.Empty;

            if (string.IsNullOrWhiteSpace(placemark.AdminArea))
                placemark.AdminArea = string.Empty;

            if (string.IsNullOrWhiteSpace(placemark.CountryName))
                placemark.CountryName = string.Empty;
            var uri = new Uri(
                $"bingmaps:?where=" +
                $"{placemark.Thoroughfare}" +
                $"%20{placemark.Locality}" +
                $"%20{placemark.AdminArea}" +
                $"%20{placemark.CountryName}");
            return uri;
        }

        static Uri CreatePointUri(double latitude, double longitude, ref MapLaunchOptions options)
        {
            var uri = new Uri(
                $"bingmaps:?collection=point." +
                $"{latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"_" +
                $"{longitude.ToString(CultureInfo.InvariantCulture)}" +
                $"_" +
                $"{options.Name}");
            return uri;
        }
    }
}
