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
        static App formsApp;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
#if SKIP_MATERIAL // We can't use Material with iOS Hot Restart as the static linker does not run
            App.PreferredVisual = Xamarin.Forms.VisualMarker.Default;
#endif

            Xamarin.Forms.Forms.Init();
            Xamarin.Forms.FormsMaterial.Init();

            Distribute.DontCheckForUpdatesInDebug();
            LoadApplication(formsApp ??= new App());

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
