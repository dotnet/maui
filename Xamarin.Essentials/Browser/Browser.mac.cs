using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options) =>
            Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(uri.AbsoluteUri)));
    }
}
