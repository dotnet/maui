using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Geocoding
    {
        static Task<IEnumerable<Placemark>> PlatformGetPlacemarksAsync(double latitude, double longitude) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<IEnumerable<Location>> PlatformGetLocationsAsync(string address) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
