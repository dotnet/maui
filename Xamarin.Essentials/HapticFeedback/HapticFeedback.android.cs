using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported => true;

        static async Task PlatformExecute(HapticFeedbackType type)
        {
            try
            {
                Platform.CurrentActivity?.Window?.DecorView?.PerformHapticFeedback(ConvertType(type));
                await Task.FromResult(0);
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
