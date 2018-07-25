using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Maps
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapsLaunchOptions options)
        {
            var uri = string.Empty;
            uri = $"geo:{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}?q={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";
            if (!string.IsNullOrWhiteSpace(options.Name))
                uri += $"({options.Name})";
            StartIntent(uri);
            return Task.CompletedTask;
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapsLaunchOptions options)
        {
            placemark = placemark.Escape();
            var uri = $"geo:0,0?q={placemark.Thoroughfare} {placemark.Locality} {placemark.AdminArea} {placemark.CountryName}";
            if (!string.IsNullOrWhiteSpace(options.Name))
                uri += $"({options.Name})";
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
