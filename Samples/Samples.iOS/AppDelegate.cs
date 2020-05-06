using System;
using Foundation;
using Microsoft.AppCenter.Distribute;
using Samples.View;
using UIKit;

namespace Samples.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Xamarin.Forms.Forms.Init();
            Xamarin.Forms.FormsMaterial.Init();
            Distribute.DontCheckForUpdatesInDebug();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (Xamarin.Essentials.Platform.OpenUrl(app, url, options))
                return true;

            return base.OpenUrl(app, url, options);
        }

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            var uri = shortcutItem.UserInfo?["uri"] as NSString;

            if (!string.IsNullOrWhiteSpace(uri))
            {
                Xamarin.Forms.Application.Current.SendOnAppLinkRequestReceived(new Uri(uri));
            }
            else
            {
                // Handle shortcuts without a Uri here

                if (shortcutItem.Type == "battery")
                {
                    Xamarin.Forms.Application.Current.MainPage.Navigation.PopToRootAsync();
                    Xamarin.Forms.Application.Current.MainPage.Navigation.PushAsync(new BatteryPage());
                }
            }
        }
    }
}
