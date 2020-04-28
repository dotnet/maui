using System;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        public static void Execute(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();
            PlatformExecute(type);
        }
    }
}
