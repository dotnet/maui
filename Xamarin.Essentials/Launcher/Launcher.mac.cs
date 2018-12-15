using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(NSWorkspace.SharedWorkspace.UrlForApplication(new NSUrl(uri.AbsoluteUri)) != null);

        static Task PlatformOpenAsync(Uri uri) =>
            Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(uri.AbsoluteUri)));
    }
}
