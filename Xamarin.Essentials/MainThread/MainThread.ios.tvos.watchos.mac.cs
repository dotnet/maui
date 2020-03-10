using System;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
        static bool PlatformIsMainThread =>
            NSThread.Current.IsMainThread;

        static void PlatformBeginInvokeOnMainThread(Action action)
        {
            NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
        }
    }
}
