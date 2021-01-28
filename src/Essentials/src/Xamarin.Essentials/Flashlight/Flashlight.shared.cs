using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Flashlight
    {
        public static Task TurnOnAsync() =>
            PlatformTurnOnAsync();

        public static Task TurnOffAsync() =>
            PlatformTurnOffAsync();
    }
}
