using Foundation;
using Microsoft.AppCenter.Distribute;
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
    }
}
