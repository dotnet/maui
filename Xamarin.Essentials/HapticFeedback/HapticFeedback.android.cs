using System;
using System.Diagnostics;
using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported => true;

        static void PlatformClick()
            => FeedBack(FeedbackConstants.ContextClick);

        static void PlatformLongPress()
             => FeedBack(FeedbackConstants.LongPress);

        static void FeedBack(FeedbackConstants feedback)
        {
            try
            {
                Platform.CurrentActivity?.Window?.DecorView?.PerformHapticFeedback(feedback);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" -- Droid -- {ex.Message}");
            }
        }
    }
}
