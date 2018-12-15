using System.Collections.Generic;

namespace Xamarin.Essentials
{
    public static partial class Connectivity
    {
        static NetworkAccess PlatformNetworkAccess =>
            throw new System.PlatformNotSupportedException();

        static IEnumerable<ConnectionProfile> PlatformConnectionProfiles =>
            throw new System.PlatformNotSupportedException();

        static void StartListeners() =>
            throw new System.PlatformNotSupportedException();

        static void StopListeners() =>
            throw new System.PlatformNotSupportedException();
    }
}
