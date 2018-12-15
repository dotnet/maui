using System;

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
        static void PlatformBeginInvokeOnMainThread(Action action) =>
            throw new System.PlatformNotSupportedException();

        static bool PlatformIsMainThread =>
            throw new System.PlatformNotSupportedException();
    }
}
