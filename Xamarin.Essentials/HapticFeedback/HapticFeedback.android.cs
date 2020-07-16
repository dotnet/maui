using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported => true;

        static void PlatformPerform(HapticFeedbackType type)
        {
            Permissions.EnsureDeclared<Permissions.Vibrate>();
            try
            {
                Platform.CurrentActivity?.Window?.DecorView?.PerformHapticFeedback(ConvertType(type));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" -- Droid -- {ex.Message}");
            }
        }

        static FeedbackConstants ConvertType(HapticFeedbackType type)
        {
            switch (type)
            {
                case HapticFeedbackType.LongPress:
                    return FeedbackConstants.LongPress;
                default:
                    return FeedbackConstants.ContextClick;
            }
        }
    }
}
