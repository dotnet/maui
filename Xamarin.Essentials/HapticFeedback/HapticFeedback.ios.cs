using System;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported => true;

        static async Task PlatformExecute(HapticFeedbackType type)
        {
            switch (type)
            {
                case HapticFeedbackType.LongPress:
                    PlatformLongPress();
                    break;
                default:
                    PlatformClick();
                    break;
            }
            await Task.CompletedTask;
        }

        static void PlatformClick()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var impact = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                impact.Prepare();
                impact.ImpactOccurred();
                impact.Dispose();
            }
        }

        static void PlatformLongPress()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var impact = new UISelectionFeedbackGenerator();
                impact.Prepare();
                impact.SelectionChanged();
                impact.Dispose();
            }
        }
    }
}
