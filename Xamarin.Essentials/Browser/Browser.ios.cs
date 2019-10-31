using System;
using System.Threading.Tasks;
using Foundation;
using SafariServices;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static async Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            var nativeUrl = new NSUrl(uri.AbsoluteUri);

            switch (options.LaunchMode)
            {
                case BrowserLaunchMode.SystemPreferred:
                    var sfViewController = new SFSafariViewController(nativeUrl, false);
                    var vc = Platform.GetCurrentViewController();

                    if (options.PreferredToolbarColor.HasValue)
                        sfViewController.PreferredBarTintColor = options.PreferredToolbarColor.Value.ToPlatformColor();

                    if (options.PreferredControlColor.HasValue)
                        sfViewController.PreferredControlTintColor = options.PreferredControlColor.Value.ToPlatformColor();

                    if (sfViewController.PopoverPresentationController != null)
                    {
                        sfViewController.PopoverPresentationController.SourceView = vc.View;
                    }
                    await vc.PresentViewControllerAsync(sfViewController, true);
                    break;
                case BrowserLaunchMode.External:
                    if (Platform.HasOSVersion(12, 0))
                    {
                        return await UIApplication.SharedApplication.OpenUrlAsync(nativeUrl, new UIApplicationOpenUrlOptions());
                    }
                    else
                    {
                        UIApplication.SharedApplication.OpenUrl(nativeUrl);
                    }
                    break;
            }

            return true;
        }
    }
}
