using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lng = longitude.ToString(CultureInfo.InvariantCulture);
            var name = options.Name ?? string.Empty;
            var uri = string.Empty;

            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"bingmaps:?collection=point.{lat}_{lng}_{name}";
            }
            else
            {
                uri = $"bingmaps:?rtp=~pos.{lat}_{lng}_{name}{GetMode(options.NavigationMode)}";
            }

            return LaunchUri(new Uri(uri));
        }

        internal static string GetMode(NavigationMode mode)
        {
            switch (mode)
            {
                case NavigationMode.Driving: return "&mode=d";
                case NavigationMode.Transit: return "&mode=t";
                case NavigationMode.Walking: return "&mode=w";
            }
            return string.Empty;
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            var uri = string.Empty;

            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"bingmaps:?where={placemark.GetEscapedAddress()}";
            }
            else
            {
                uri = $"bingmaps:?rtp=~adr.{placemark.GetEscapedAddress()}{GetMode(options.NavigationMode)}";
            }

            return LaunchUri(new Uri(uri));
        }

        static Task LaunchUri(Uri mapsUri) =>
            Windows.System.Launcher.LaunchUriAsync(mapsUri).AsTask();
    }
}
