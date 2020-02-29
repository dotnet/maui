using System;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformClick()
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformLongPress()
             => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
