using System;
using System.Threading.Tasks;
using Foundation;
using SafariServices;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static async Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchMode launchMode)
        {
            var nativeUrl = new NSUrl(uri.AbsoluteUri);

            switch (launchMode)
            {
                case BrowserLaunchMode.SystemPreferred:
                    var sfViewController = new SFSafariViewController(nativeUrl, false);
                    var vc = Platform.GetCurrentViewController();

                    if (sfViewController.PopoverPresentationController != null)
                    {
                        sfViewController.PopoverPresentationController.SourceView = vc.View;
                    }
                    await vc.PresentViewControllerAsync(sfViewController, true);
                    break;
                case BrowserLaunchMode.External:
                    return await UIApplication.SharedApplication.OpenUrlAsync(nativeUrl, new UIApplicationOpenUrlOptions());
            }

            return true;
        }
    }
}
