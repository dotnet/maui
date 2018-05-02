using UIKit;

namespace Xamarin.Essentials
{
    public static partial class ScreenLock
    {
        static bool PlatformIsActive
            => UIApplication.SharedApplication.IdleTimerDisabled;

        static void PlatformRequestActive()
            => UIApplication.SharedApplication.IdleTimerDisabled = true;

        static void PlatformRequestRelease()
            => UIApplication.SharedApplication.IdleTimerDisabled = false;
    }
}
