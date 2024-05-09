using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;
using Microsoft.Maui;
using System.Linq;
using System;

#if IOS
using UIKit;
using CoreGraphics;
#endif

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16321, "Alerts Open on top of current presented view", PlatformAffected.All)]
	public class Issue16321NavPage : NavigationPage
	{
		public Issue16321NavPage() : base(new Issue16321())
		{

		}
	}

	public partial class Issue16321 : ContentPage
	{
		public Issue16321() : base()
		{
			InitializeComponent();
		}

		async void OpenAlertWithModals(System.Object sender, System.EventArgs e)
		{
			await Navigation.PushModalAsync(new ContentPage());
			await Navigation.PushModalAsync(new ContentPage());
			await this.DisplayAlert("hello", "message", "Cancel");
			await Navigation.PopModalAsync();
			await Navigation.PopModalAsync();
		}

#if IOS
		async void OpenAlertWithNewUIWindow(System.Object sender, System.EventArgs e)
		{
			var uIWindow = new UIWindow();
			var keyWindow = (this.Window.Handler.PlatformView as UIWindow);
			if (keyWindow?.WindowLevel == UIWindowLevel.Normal)
				keyWindow.WindowLevel = -1;

			var page = new ContentPage();
			this.AddLogicalChild(page);
			var handler = page.ToHandler(this.Handler.MauiContext);
			var popupVC = handler.ViewController;

			uIWindow.RootViewController = new UIViewController();
			uIWindow.WindowLevel = UIWindowLevel.Normal;
			uIWindow.MakeKeyAndVisible();

			popupVC.ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
			popupVC.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
			await uIWindow.RootViewController.PresentViewControllerAsync(popupVC, false);

			await page.DisplayAlert("hello", "message", "Cancel");

			var rvc = uIWindow.RootViewController;

			if (rvc != null)
			{
				await rvc.DismissViewControllerAsync(false);
				rvc.Dispose();
			}

			uIWindow.RootViewController = null;
			uIWindow.Hidden = true;
			keyWindow.WindowLevel = UIWindowLevel.Normal;
			this.RemoveLogicalChild(page);
		}
#else
			async void OpenAlertWithNewUIWindow(System.Object sender, System.EventArgs e)
			{
				await this.DisplayAlert("hello", "message", "Cancel");
			}
#endif
	}
}