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
	public Issue21726()
	{
		InitializeComponent();
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

			var testNC = new UIKit.UINavigationController(testVC)
			{
				ModalPresentationStyle = UIKit.UIModalPresentationStyle.FullScreen
			};

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

			TextField1 = new UIKit.UITextField(new CoreGraphics.CGRect(20, 120, 200, 20))
			{
				Placeholder = "TextField1",
				BorderStyle = UIKit.UITextBorderStyle.RoundedRect,
				AccessibilityIdentifier = "TextField1"
			};

			Button1 = new UIKit.UIButton(new CoreGraphics.CGRect(20, 320, 200, 40));
			Button1.SetTitle("Dismiss", UIKit.UIControlState.Normal);
			Button1.BackgroundColor = UIKit.UIColor.SystemBlue;
			Button1.AccessibilityIdentifier = "Button1";
			Button1.TouchUpInside += (sender, e) => {
				DismissViewController(true, null);
			};

			View.AddSubview(TextField1);
			View.AddSubview(Button1);
		}
	}
#endif
}
