using System;

namespace Xamarin.Essentials
{
    public static partial class Vibration
    {
        internal static bool IsSupported
            => throw new NotImplementedInReferenceAssemblyException();

        static void PlatformVibrate(TimeSpan duration)
            => throw new NotImplementedInReferenceAssemblyException();

        static void PlatformCancel()
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
