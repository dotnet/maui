using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Flashlight
    {
        static Task PlatformTurnOnAsync() =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformTurnOffAsync() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
