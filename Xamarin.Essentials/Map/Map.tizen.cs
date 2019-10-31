using System.Globalization;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            Permissions.EnsureDeclared(PermissionType.LaunchApp);

            var appControl = new AppControl
            {
                Operation = AppControlOperations.View,
                Uri = "geo:",
            };

            appControl.Uri += $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            Permissions.EnsureDeclared(PermissionType.LaunchApp);

            var appControl = new AppControl
            {
                Operation = AppControlOperations.Pick,
                Uri = "geo:",
            };

            placemark = placemark.Escape();
            appControl.Uri += $"0,0?q={placemark.Thoroughfare} {placemark.Locality} {placemark.AdminArea} {placemark.PostalCode} {placemark.CountryName}";

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }
    }
}
