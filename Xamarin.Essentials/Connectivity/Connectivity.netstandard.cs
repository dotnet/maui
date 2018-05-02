using System.Collections.Generic;

namespace Xamarin.Essentials
{
    public static partial class Connectivity
    {
        static NetworkAccess PlatformNetworkAccess =>
            throw new NotImplementedInReferenceAssemblyException();

        static IEnumerable<ConnectionProfile> PlatformProfiles =>
            throw new NotImplementedInReferenceAssemblyException();

        static void StartListeners() =>
            throw new NotImplementedInReferenceAssemblyException();

        static void StopListeners() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
