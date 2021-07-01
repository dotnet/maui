using System;
using Foundation;
using Microsoft.AppCenter.Distribute;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Samples.View;
using UIKit;

namespace Samples.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : MauiUIApplicationDelegate<Startup>
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Distribute.DontCheckForUpdatesInDebug();

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (Microsoft.Maui.Essentials.Platform.OpenUrl(app, url, options))
                return true;

            return base.OpenUrl(app, url, options);
        }

        public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
        {
            if (Microsoft.Maui.Essentials.Platform.ContinueUserActivity(application, userActivity, completionHandler))
                return true;

            return base.ContinueUserActivity(application, userActivity, completionHandler);
        }

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
            => Microsoft.Maui.Essentials.Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler);
    }
}
