using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;
using Microsoft.Maui;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21948, "Crash upon resuming the app window was already activated", PlatformAffected.iOS)]
	public partial class Issue21948 : ContentPage
	{
		public Issue21948()
		{
			InitializeComponent();
		}

		public void OpenNewWindowClicked(object obj, EventArgs e)
		{
#if IOS || MACCATALYST
			OpenNewWindow();
#endif
		}

#if IOS || MACCATALYST
		async void OpenNewWindow()
        {
            var uIWindow = new UIWindow();
            var keyWindow = (this.Window.Handler.PlatformView as UIWindow);
            if (keyWindow?.WindowLevel == UIWindowLevel.Normal)
                keyWindow.WindowLevel = -1;

            var page = new ContentPage();
            this.AddLogicalChild(page);
            var handler = page.ToHandler(this.Handler.MauiContext);

            uIWindow.RootViewController = new UIViewController();
            uIWindow.WindowLevel = UIWindowLevel.Normal;
            uIWindow.MakeKeyAndVisible();

            // Simulate backgrounding the app
            nint taskId = UIApplication.BackgroundTaskInvalid;
            taskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
            {
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });

			// Simulate background time
            await Task.Delay(2000); 
            UIApplication.SharedApplication.EndBackgroundTask(taskId);

			var rvc = uIWindow.RootViewController;

            if (rvc != null)
            {
                await rvc.DismissViewControllerAsync(false);
                rvc.Dispose();
            }

            // Simulate bringing the app back to the foreground
            await Task.Delay(1000);

            uIWindow.RootViewController = null;
            uIWindow.Hidden = true;
            keyWindow.WindowLevel = UIWindowLevel.Normal;
            this.RemoveLogicalChild(page);
		}
#endif
	}

}