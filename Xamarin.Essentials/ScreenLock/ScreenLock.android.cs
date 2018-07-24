using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class ScreenLock
    {
        static bool PlatformIsActive
        {
            get
            {
                var activity = Platform.GetCurrentActivity(true);
                var flags = activity.Window?.Attributes?.Flags ?? 0;
                return flags.HasFlag(WindowManagerFlags.KeepScreenOn);
            }
        }

        static void PlatformRequestActive()
        {
            var activity = Platform.GetCurrentActivity(true);
            activity.Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        static void PlatformRequestRelease()
        {
            var activity = Platform.GetCurrentActivity(true);
            activity.Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
        }
    }
}
