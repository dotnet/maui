using Android.Views;

namespace Microsoft.Caboodle
{
    public static partial class ScreenLock
    {
        public static bool IsActive
        {
            get
            {
                var activity = Platform.CurrentActivity;
                var flags = activity?.Window?.Attributes?.Flags ?? 0;
                return flags.HasFlag(WindowManagerFlags.KeepScreenOn);
            }
        }

        public static void RequestActive()
        {
            var activity = Platform.CurrentActivity;
            activity?.Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        public static void RequestRelease()
        {
            var activity = Platform.CurrentActivity;
            activity?.Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
        }
    }
}
