using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        static Task<Location> PlatformLastKnownLocationAsync() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
