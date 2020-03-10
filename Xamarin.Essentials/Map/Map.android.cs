using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = string.Empty;
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lng = longitude.ToString(CultureInfo.InvariantCulture);

            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"geo:{lat},{lng}?q={lat},{lng}";

                if (!string.IsNullOrWhiteSpace(options.Name))
                    uri += $"({AndroidUri.Encode(options.Name)})";
            }
            else
            {
                uri = $"google.navigation:q={lat},{lng}{GetMode(options.NavigationMode)}";
            }

            StartIntent(uri);
            return Task.CompletedTask;
        }

        internal static string GetMode(NavigationMode mode)
        {
            switch (mode)
            {
                case NavigationMode.Bicycling: return "&mode=b";
                case NavigationMode.Driving: return "&mode=d";
                case NavigationMode.Walking: return "&mode=w";
            }
            return string.Empty;
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            placemark = placemark.Escape();
            var uri = string.Empty;
            if (options.NavigationMode == NavigationMode.None)
            {
                uri = $"geo:0,0?q={placemark.Thoroughfare} {placemark.Locality} {placemark.AdminArea} {placemark.PostalCode} {placemark.CountryName}";
                if (!string.IsNullOrWhiteSpace(options.Name))
                    uri += $"({AndroidUri.Encode(options.Name)})";
            }
            else
            {
                uri = $"google.navigation:q={placemark.Thoroughfare} {placemark.Locality} {placemark.AdminArea} {placemark.PostalCode} {placemark.CountryName}{GetMode(options.NavigationMode)}";
            }

            StartIntent(uri);
            return Task.CompletedTask;
        }

        static void StartIntent(string uri)
        {
            var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri));
            var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
#if __ANDROID_24__
            if (Platform.HasApiLevelN)
                flags |= ActivityFlags.LaunchAdjacent;
#endif
            intent.SetFlags(flags);

            Platform.AppContext.StartActivity(intent);
        }
    }
}
