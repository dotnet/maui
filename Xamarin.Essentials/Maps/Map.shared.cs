using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        public static Task OpenMapsAsync(Location location, MapLaunchOptions options)
            => PlatformOpenMapsAsync(location, options);

        public static Task OpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
            => PlatformOpenMapsAsync(latitude, longitude, options);

        public static Task OpenMapsAsync(Placemark placemark, MapLaunchOptions options)
            => PlatformOpenMapsAsync(placemark, options);
    }
}
