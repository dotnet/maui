#nullable disable
using System;
using Foundation;
using UIKit;
using WebKit;

namespace Microsoft.Maui.Platform
{
	class MauiWebViewUIDelegate : WKUIDelegate
	{
		static readonly string LocalOK = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");
		static readonly string LocalCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");

		public override void RunJavaScriptAlertPanel(WKWebView webView, string message, WKFrameInfo frame, Action completionHandler)
		{
			PresentAlertController(
				webView,
				message,
				okAction: _ => completionHandler()
			);
		}

		public override void RunJavaScriptConfirmPanel(WKWebView webView, string message, WKFrameInfo frame, Action<bool> completionHandler)
		{
			PresentAlertController(
				webView,
				message,
				okAction: _ => completionHandler(true),
				cancelAction: _ => completionHandler(false)
			);
		}

		public override void RunJavaScriptTextInputPanel(
			WKWebView webView, string prompt, string defaultText, WKFrameInfo frame, Action<string> completionHandler)
		{
			PresentAlertController(
				webView,
				prompt,
				defaultText: defaultText,
				okAction: x => completionHandler(x.TextFields[0].Text),
				cancelAction: _ => completionHandler(null)
			);
		}

		static string GetJsAlertTitle(WKWebView webView)
		{
			// Emulate the behavior of UIWebView dialogs.
			// The scheme and host are used unless local html content is what the webview is displaying,
			// in which case the bundle file name is used.

			if (webView.Url != null && webView.Url.AbsoluteString != $"file://{NSBundle.MainBundle.BundlePath}/")
				return $"{webView.Url.Scheme}://{webView.Url.Host}";

			return new NSString(NSBundle.MainBundle.BundlePath).LastPathComponent;
		}

		static UIAlertAction AddOkAction(UIAlertController controller, Action handler)
		{
			var action = UIAlertAction.Create(LocalOK, UIAlertActionStyle.Default, (_) => handler());
			controller.AddAction(action);
			controller.PreferredAction = action;
			return action;
		}

		static UIAlertAction AddCancelAction(UIAlertController controller, Action handler)
		{
			var action = UIAlertAction.Create(LocalCancel, UIAlertActionStyle.Cancel, (_) => handler());
			controller.AddAction(action);
			return action;
		}

		static void PresentAlertController(
			WKWebView webView,
			string message,
			string defaultText = null,
			Action<UIAlertController> okAction = null,
			Action<UIAlertController> cancelAction = null)
		{
			var controller = UIAlertController.Create(GetJsAlertTitle(webView), message, UIAlertControllerStyle.Alert);

			if (defaultText != null)
				controller.AddTextField((textField) => textField.Text = defaultText);

			if (okAction != null)
				AddOkAction(controller, () => okAction(controller));

			if (cancelAction != null)
				AddCancelAction(controller, () => cancelAction(controller));

			GetTopViewController(UIApplication.SharedApplication.GetKeyWindow().RootViewController)
				.PresentViewController(controller, true, null);
		}

		static UIViewController GetTopViewController(UIViewController viewController)
		{
			if (viewController is UINavigationController navigationController)
				return GetTopViewController(navigationController.VisibleViewController);

			if (viewController is UITabBarController tabBarController)
				return GetTopViewController(tabBarController.SelectedViewController);

			if (viewController.PresentedViewController != null)
				return GetTopViewController(viewController.PresentedViewController);

			return viewController;
		}
	}
}