using System;
using System.Linq;
using Foundation;
using UIKit;

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

        internal static bool HasOSVersion(int major, int minor) =>
            UIDevice.CurrentDevice.CheckSystemVersion(major, minor);

        internal static UIViewController GetCurrentViewController(bool throwIfNull = true)
        {
            UIViewController viewController = null;

            var window = UIApplication.SharedApplication.KeyWindow;

            if (window.WindowLevel == UIWindowLevel.Normal)
                viewController = window.RootViewController;

            if (viewController == null)
            {
                window = UIApplication.SharedApplication
                    .Windows
                    .OrderByDescending(w => w.WindowLevel)
                    .FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);

                if (window == null)
                    throw new InvalidOperationException("Could not find current view controller.");
                else
                    viewController = window.RootViewController;
            }

            while (viewController.PresentedViewController != null)
                viewController = viewController.PresentedViewController;

            if (throwIfNull && viewController == null)
                throw new InvalidOperationException("Could not find current view controller.");

            return viewController;
        }
    }
}
