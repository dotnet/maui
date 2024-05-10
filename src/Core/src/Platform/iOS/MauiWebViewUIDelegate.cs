using System;
using Foundation;
using UIKit;
using WebKit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebViewUIDelegate : WKUIDelegate
	{
		WeakReference<IWebViewHandler> _handler;
		static readonly string LocalOK = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");
		static readonly string LocalCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");

		public MauiWebViewUIDelegate(IWebViewHandler handler)
		{
			_ = handler ?? throw new ArgumentNullException("handler");
			_handler = new WeakReference<IWebViewHandler>(handler);
		}

		public override void SetContextMenuConfiguration(WKWebView webView, WKContextMenuElementInfo elementInfo, Action<UIContextMenuConfiguration> completionHandler)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			UIContextMenuConfiguration? uIContextMenuConfiguration = null;
			foreach (var interaction in webView.Interactions)
			{
				if (interaction is MauiUIContextMenuInteraction cmi)
				{
					var contextMenu = cmi.GetConfigurationForMenu();
					if (contextMenu != null)
					{
						uIContextMenuConfiguration = contextMenu;
					}

					break;
				}
			}
			completionHandler(uIContextMenuConfiguration!);
			return;
		}

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
			WKWebView webView, string prompt, string? defaultText, WKFrameInfo frame, Action<string> completionHandler)
		{
			// TODO the okAction and cancelAction were already taking null when I removed `#nullable disable`
			// and it doesn't seem to have been causing issues so leaving for now
			PresentAlertController(
				webView,
				prompt,
				defaultText: defaultText,
				okAction: x => completionHandler(x.TextFields[0].Text!),
				cancelAction: _ => completionHandler(null!)
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
			string? defaultText = null,
			Action<UIAlertController>? okAction = null,
			Action<UIAlertController>? cancelAction = null)
		{
			var controller = UIAlertController.Create(GetJsAlertTitle(webView), message, UIAlertControllerStyle.Alert);

			if (defaultText != null)
				controller.AddTextField((textField) => textField.Text = defaultText);

			if (okAction != null)
				AddOkAction(controller, () => okAction(controller));

			if (cancelAction != null)
				AddCancelAction(controller, () => cancelAction(controller));


			var handler = (webView.UIDelegate as MauiWebViewUIDelegate)?.Handler;

			UIViewController? rootViewController = null;

			if (handler != null)
				rootViewController = handler.MauiContext?.GetPlatformWindow()?.RootViewController;

			rootViewController ??= UIApplication.SharedApplication?.GetKeyWindow()?.RootViewController;

			if (rootViewController != null)
			{
				GetTopViewController(rootViewController)
					.PresentViewController(controller, true, null);
			}
		}

		static UIViewController GetTopViewController(UIViewController viewController)
		{
			if (viewController is UINavigationController navigationController)
				return GetTopViewController(navigationController.VisibleViewController);

			if (viewController is UITabBarController tabBarController && tabBarController.SelectedViewController != null)
				return GetTopViewController(tabBarController.SelectedViewController);

			if (viewController.PresentedViewController != null)
				return GetTopViewController(viewController.PresentedViewController);

			return viewController;
		}

		IWebViewHandler? Handler
		{
			get
			{
				if (_handler.TryGetTarget(out var handler))
				{
					return handler;
				}

				return null;
			}
		}
	}
}