using UIKit;

namespace Xamarin.Essentials
{
    public static partial class ScreenLock
    {
        public static bool IsActive
            => UIApplication.SharedApplication.IdleTimerDisabled;

        public static void RequestActive()
        {
            UIApplication.SharedApplication.IdleTimerDisabled = true;
        }

        public static void RequestRelease()
        {
            UIApplication.SharedApplication.IdleTimerDisabled = false;
        }
    }
}
