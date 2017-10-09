using CoreGraphics;
using Embedding.XF;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Embedding.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public static AppDelegate Shared;
		public static UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);

		UIViewController _hello;
		UIViewController _alertsAndActionSheets;
		UIViewController _webview;

		UIBarButtonItem _helloButton;

		UIWindow _window;
		UINavigationController _navigation;
		ViewController _mainController;

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Forms.Init();

			Shared = this;
			_window = new UIWindow(UIScreen.MainScreen.Bounds);

			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes
			{
				TextColor = UIColor.White
			});

			_mainController = Storyboard.InstantiateInitialViewController() as ViewController;
			_navigation = new UINavigationController(_mainController);

			_window.RootViewController = _navigation;
			_window.MakeKeyAndVisible();

			return true;
		}

		public UIBarButtonItem CreateHelloButton()
		{
			if (_helloButton == null)
			{
				var btn = new UIButton(new CGRect(0, 0, 88, 44));
				btn.SetTitle("Hello", UIControlState.Normal);
				btn.SetTitleColor(UIColor.Blue, UIControlState.Normal);
				btn.TouchUpInside += (sender, e) => ShowHello();

				_helloButton = new UIBarButtonItem(btn);
			}

			return _helloButton;
		}

		public void ShowHello()
		{
			if (_hello == null)
			{
				_hello = new Hello().CreateViewController();
			}

			_navigation.PushViewController(_hello, true);
		}

		public void ShowAlertsAndActionSheets()
		{
			if (_alertsAndActionSheets == null)
			{
				_alertsAndActionSheets = new AlertsAndActionSheets().CreateViewController();
			}

			_navigation.PushViewController(_alertsAndActionSheets, true);
		}

		public void ShowWebView()
		{
			if (_webview == null)
			{
				_webview = new WebViewExample().CreateViewController();
			}

			_navigation.PushViewController(_webview, true);
		}
	}
}