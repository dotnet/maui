using System;
using Foundation;

namespace Microsoft.Caboodle
{
    public static partial class Platform
    {
        public static void BeginInvokeOnMainThread(Action action)
        {
            if (NSThread.Current.IsMainThread)
            {
                action();
                return;
            }

            NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
        }
    }
}
