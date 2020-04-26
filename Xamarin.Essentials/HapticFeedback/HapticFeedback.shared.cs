using System;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        public static async void Execute(HapticFeedbackType type = HapticFeedbackType.Click)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();
            await PlatformExecute(type);
        }
    }
}
