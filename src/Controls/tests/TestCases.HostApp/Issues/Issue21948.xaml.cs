using Microsoft.Maui.Platform;
#if IOS || MACCATALYST
using UIKit;
using CoreGraphics;
#endif

namespace Maui.Controls.Sample.Issues
{
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
			
			var keyWindow = (this.Window.Handler.PlatformView as UIWindow);
			if (keyWindow?.WindowLevel == UIWindowLevel.Normal)
				keyWindow.WindowLevel = -1;

			UIWindow uIWindow;
			if (OperatingSystem.IsIOSVersionAtLeast(13) && keyWindow?.WindowScene is not null)
			{
				uIWindow = new UIWindow(keyWindow.WindowScene);
			}
			else
			{
#pragma warning disable CA1422 // This call site is reachable on iOS < 13.0
				uIWindow = new UIWindow();
#pragma warning restore CA1422
			}

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