using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(NSWorkspace.SharedWorkspace.UrlForApplication(GetNativeUrl(uri)) != null);

        static Task PlatformOpenAsync(Uri uri) =>
            Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(GetNativeUrl(uri)));

        static Task<bool> PlatformTryOpenAsync(Uri uri)
        {
            var nativeUrl = GetNativeUrl(uri);
            var canOpen = NSWorkspace.SharedWorkspace.UrlForApplication(nativeUrl) != null;

            if (canOpen)
            {
                // TODO: there is an OpenUrlAsync that may be useful
                return Task.FromResult(NSWorkspace.SharedWorkspace.OpenUrl(nativeUrl));
            }

            return Task.FromResult(canOpen);
        }

        internal static NSUrl GetNativeUrl(Uri uri)
        {
            try
            {
                return new NSUrl(uri.OriginalString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to create Url from Original string, try absolute Uri: " + ex.Message);
                return new NSUrl(uri.AbsoluteUri);
            }
        }

        static Task PlatformOpenAsync(OpenFileRequest request) =>
            Task.FromResult(NSWorkspace.SharedWorkspace.OpenFile(request.File.FullPath));
    }
}
