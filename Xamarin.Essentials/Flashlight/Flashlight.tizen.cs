using System.Threading.Tasks;
using Tizen.System;

namespace Xamarin.Essentials
{
    public static partial class Flashlight
    {
        internal static bool IsSupported
            => Platform.GetFeatureInfo<bool>("camera.back.flash");

        internal static Task SwitchFlashlight(bool switchOn)
        {
            Permissions.EnsureDeclared(PermissionType.Flashlight);
            return Task.Run(() =>
            {
                if (!IsSupported)
                    throw new FeatureNotSupportedException();

                if (switchOn)
                    Led.Brightness = Led.MaxBrightness;
                else
                    Led.Brightness = 0;
            });
        }

        static Task PlatformTurnOnAsync()
        {
            return SwitchFlashlight(true);
        }

        static Task PlatformTurnOffAsync()
        {
            return SwitchFlashlight(false);
        }
    }
}
