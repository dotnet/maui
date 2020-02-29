using System;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        public static void Execute(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();
            switch (type)
            {
                case HapticFeedbackType.Click:
                    PlatformClick();
                    break;
                case HapticFeedbackType.LongPress:
                    PlatformLongPress();
                    break;
            }
        }
    }
}
