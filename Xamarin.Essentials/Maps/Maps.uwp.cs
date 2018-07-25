using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Maps
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapsLaunchOptions options)
        {
            return LaunchUri(new Uri(
                $"bingmaps:?collection=point." +
                $"{latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"_" +
                $"{longitude.ToString(CultureInfo.InvariantCulture)}" +
                $"_" +
                $"{options.Name ?? string.Empty}"));
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapsLaunchOptions options)
        {
            placemark = placemark.Escape();

            var uri = new Uri(
                $"bingmaps:?where=" +
                $"{placemark.Thoroughfare}" +
                $"%20{placemark.Locality}" +
                $"%20{placemark.AdminArea}" +
                $"%20{placemark.CountryName}");

            return LaunchUri(uri);
        }

        static Task LaunchUri(Uri mapsUri) =>
            Windows.System.Launcher.LaunchUriAsync(mapsUri).AsTask();
    }
}
