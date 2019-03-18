using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(UIApplication.SharedApplication.CanOpenUrl(new NSUrl(uri.OriginalString)));

        static Task PlatformOpenAsync(Uri uri) =>
            UIApplication.SharedApplication.OpenUrlAsync(new NSUrl(uri.OriginalString), new UIApplicationOpenUrlOptions());
    }
}
