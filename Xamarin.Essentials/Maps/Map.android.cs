using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(Location location, MapLaunchOptions options)
            => PlatformOpenMapsAsync(location.Latitude, location.Longitude, options);

        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var uri = string.Empty;
            if (string.IsNullOrWhiteSpace(options.Name))
                uri = $"geo:{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}?q={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";
            else
                uri = $"geo:{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}?q={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}({options.Name})";
            StartIntent(uri);
            return Task.CompletedTask;
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
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
            var uri = $"http://maps.google.com/maps?q={placemark.Thoroughfare} {placemark.Locality}, {placemark.AdminArea} {placemark.CountryName}";
            StartIntent(uri);
            return Task.CompletedTask;
        }

        static void StartIntent(string uri)
        {
            var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri));
            intent.SetFlags(ActivityFlags.ClearTop);
            intent.SetFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(intent);
        }
    }
}
