using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(UIApplication.SharedApplication.CanOpenUrl(GetNativeUrl(uri)));

        static Task PlatformOpenAsync(Uri uri) =>
            UIApplication.SharedApplication.OpenUrlAsync(GetNativeUrl(uri), new UIApplicationOpenUrlOptions());

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
    }
}
