using System;

namespace Xamarin.Essentials
{
    public static partial class Vibration
    {
        public static void Vibrate()
            => Vibrate(TimeSpan.FromMilliseconds(500));

        public static void Vibrate(double duration)
            => Vibrate(TimeSpan.FromMilliseconds(duration));

        public static void Vibrate(TimeSpan duration)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (duration.TotalMilliseconds < 0)
                duration = TimeSpan.Zero;
            else if (duration.TotalSeconds > 5)
                duration = TimeSpan.FromSeconds(5);

            PlatformVibrate(duration);
        }

        public static void Cancel()
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            PlatformCancel();
        }
    }
}
