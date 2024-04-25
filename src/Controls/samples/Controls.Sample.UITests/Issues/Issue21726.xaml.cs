using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21726, "Modal with a bottom sheet should not crash iOS Keyboard Scroll", PlatformAffected.iOS)]
public partial class Issue21726 : ContentPage
{
	public static bool Success = false;
	public Issue21726()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (Success)
			successLabel.IsVisible = true;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Success = false;
	}

	void AddVC (object sender, EventArgs e)
	{
#if IOS
		var window = UIKit.UIApplication.SharedApplication
        .ConnectedScenes
        .OfType<UIKit.UIWindowScene>()
        .SelectMany(s => s.Windows)
        .FirstOrDefault(w => w.IsKeyWindow);

		var rootVC = window?.RootViewController;
		while (rootVC?.PresentedViewController != null)
		{
			rootVC = rootVC.PresentedViewController;
		}

		if (rootVC is not null) {
			var testVC = new TestViewController();

			var testNC = new UIKit.UINavigationController(testVC);

			testNC.ModalPresentationStyle = UIKit.UIModalPresentationStyle.FullScreen;

			rootVC.PresentViewController(testNC, true, null);
		}
#endif
	}

#if IOS
	public class TestViewController: UIKit.UIViewController {

		UIKit.UITextField TextField1;
		UIKit.UIButton Button1;

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			View.BackgroundColor = UIKit.UIColor.White;

			TextField1 = new UIKit.UITextField(new CoreGraphics.CGRect(20, 120, 200, 20));
			TextField1.Placeholder = "TextField1";
			TextField1.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;

			Button1 = new UIKit.UIButton(new CoreGraphics.CGRect(20, 320, 200, 40));
			Button1.SetTitle("Dismiss", UIKit.UIControlState.Normal);
			Button1.BackgroundColor = UIKit.UIColor.SystemBlue;
			Button1.TouchUpInside += (sender, e) => {
				DismissViewController(true, null);
			};

			View.AddSubview(TextField1);
			View.AddSubview(Button1);
		}

		public override async void ViewDidAppear(bool animated) {
			try
			{
				base.ViewDidAppear(animated);
				TextField1.BecomeFirstResponder();
				await Task.Delay(1000);
				TextField1.ResignFirstResponder();
				await Task.Delay(1000);
				Button1.SendActionForControlEvents(UIKit.UIControlEvent.TouchUpInside);
				Issue21726.Success = true;
			}
			catch
			{
				Issue21726.Success = false;
			}
		}
	}
#endif
}
