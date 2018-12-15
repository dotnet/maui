using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Flashlight
    {
        static Task PlatformTurnOnAsync() =>
            throw new System.PlatformNotSupportedException();

        static Task PlatformTurnOffAsync() =>
            throw new System.PlatformNotSupportedException();
    }
}
