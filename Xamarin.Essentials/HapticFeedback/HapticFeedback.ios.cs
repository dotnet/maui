using System;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported => true;

        static void PlatformPerform(HapticFeedbackType type)
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
