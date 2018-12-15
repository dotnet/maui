using System;

namespace Xamarin.Essentials
{
    public static partial class Vibration
    {
        internal static bool IsSupported
            => throw new System.PlatformNotSupportedException();

        static void PlatformVibrate(TimeSpan duration)
            => throw new System.PlatformNotSupportedException();

        static void PlatformCancel()
            => throw new System.PlatformNotSupportedException();
    }
}
