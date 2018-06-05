using System;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        static void PlatformBeginInvokeOnMainThread(Action action) =>
            throw new NotImplementedInReferenceAssemblyException();

        static bool PlatformIsMainThread =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
