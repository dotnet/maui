using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class LocationExtensions
    {
        public static double CalculateDistance(this Location locationStart, double latitudeEnd, double longitudeEnd, DistanceUnits units) =>
            Location.CalculateDistance(locationStart, latitudeEnd, longitudeEnd, units);

        public static double CalculateDistance(this Location locationStart, Location locationEnd, DistanceUnits units) =>
            Location.CalculateDistance(locationStart, locationEnd, units);

        public static Task OpenMapsAsync(this Location location, MapLaunchOptions options) =>
            Map.OpenAsync(location, options);

        public static Task OpenMapsAsync(this Location location) =>
            Map.OpenAsync(location);
    }
}
