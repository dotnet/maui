using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Maps
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapsLaunchOptions options)
        {
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lng = longitude.ToString(CultureInfo.InvariantCulture);
            var name = options.Name ?? string.Empty;
            var uri = string.Empty;

            if (options.MapDirectionsMode == MapDirectionsMode.None)
            {
                uri = $"bingmaps:?collection=point.{lat}_{lng}_{name}";
            }
            else
            {
                uri = $"bingmaps:?rtp=~pos.{lat}_{lng}_{name}{GetMode(options.MapDirectionsMode)}";
            }

            return LaunchUri(new Uri(uri));
        }

        internal static string GetMode(MapDirectionsMode mode)
        {
            switch (mode)
            {
                case MapDirectionsMode.Driving: return "&mode=d";
                case MapDirectionsMode.Transit: return "&mode=t";
                case MapDirectionsMode.Walking: return "&mode=w";
            }
            return string.Empty;
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapsLaunchOptions options)
        {
            placemark = placemark.Escape();
            var uri = string.Empty;

            if (options.MapDirectionsMode == MapDirectionsMode.None)
            {
                uri = $"bingmaps:?where=" +
                    $"{placemark.Thoroughfare}" +
                    $"%20{placemark.Locality}" +
                    $"%20{placemark.AdminArea}" +
                    $"%20{placemark.PostalCode}" +
                    $"%20{placemark.CountryName}";
            }
            else
            {
                uri = $"bingmaps:?rtp=~adr." +
                    $"{placemark.Thoroughfare}" +
                    $"%20{placemark.Locality}" +
                    $"%20{placemark.AdminArea}" +
                    $"%20{placemark.PostalCode}" +
                    $"%20{placemark.CountryName}" +
                    $"{GetMode(options.MapDirectionsMode)}";
            }

            return LaunchUri(new Uri(uri));
        }

        static Task LaunchUri(Uri mapsUri) =>
            Windows.System.Launcher.LaunchUriAsync(mapsUri).AsTask();
    }
}
