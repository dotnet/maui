using System;

namespace Xamarin.Essentials
{
    public static partial class Vibration
    {
        internal static bool IsSupported =>
            throw new PlatformNotSupportedException();

        static void PlatformVibrate(TimeSpan duration) =>
            throw new PlatformNotSupportedException();

        static void PlatformCancel() =>
            throw new PlatformNotSupportedException();
    }
}
