using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Maps
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapsLaunchOptions options)
            => throw new NotImplementedInReferenceAssemblyException();

        internal static Task PlatformOpenMapsAsync(Placemark placemark, MapsLaunchOptions options)
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
