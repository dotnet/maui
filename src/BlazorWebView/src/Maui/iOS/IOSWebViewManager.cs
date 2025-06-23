using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using CoreFoundation;
using Foundation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using WebKit;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// An implementation of <see cref="WebViewManager"/> that uses the <see cref="WKWebView"/> browser control
	/// to render web content.
	/// </summary>
	internal class IOSWebViewManager : WebViewManager
	{
		private readonly BlazorWebViewHandler _blazorMauiWebViewHandler;
		private readonly ILogger _logger;
		private readonly WKWebView _webview;
		private readonly string _contentRootRelativeToAppRoot;

		/// <summary>
		/// Initializes a new instance of <see cref="IOSWebViewManager"/>
		/// </summary>
		/// <param name="blazorMauiWebViewHandler">The <see cref="BlazorWebViewHandler"/>.</param>
		/// <param name="webview">The <see cref="WKWebView"/> to render web content in.</param>
		/// <param name="provider">The <see cref="IServiceProvider"/> for the application.</param>
		/// <param name="dispatcher">A <see cref="Dispatcher"/> instance instance that can marshal calls to the required thread or sync context.</param>
		/// <param name="fileProvider">Provides static content to the webview.</param>
		/// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
		/// <param name="contentRootRelativeToAppRoot">Path to the directory containing application content files.</param>
		/// <param name="hostPageRelativePath">Path to the host page within the fileProvider.</param>
		/// <param name="logger">Logger to send log messages to.</param>

		public IOSWebViewManager(BlazorWebViewHandler blazorMauiWebViewHandler, WKWebView webview, IServiceProvider provider, Dispatcher dispatcher, IFileProvider fileProvider, JSComponentConfigurationStore jsComponents, string contentRootRelativeToAppRoot, string hostPageRelativePath, ILogger logger)
			: base(provider, dispatcher, BlazorWebViewHandler.AppOriginUri, fileProvider, jsComponents, hostPageRelativePath)
		{
			ArgumentNullException.ThrowIfNull(blazorMauiWebViewHandler);
			ArgumentNullException.ThrowIfNull(webview);

			if (provider.GetService<MauiBlazorMarkerService>() is null)
			{
				throw new InvalidOperationException(
					"Unable to find the required services. " +
					$"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(BlazorWebViewServiceCollectionExtensions.AddMauiBlazorWebView)}' in the application startup code.");
			}

			_logger = logger;
			_blazorMauiWebViewHandler = blazorMauiWebViewHandler;
			_webview = webview;
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

			InitializeWebView();
		}

		/// <inheritdoc />
		protected override void NavigateCore(Uri absoluteUri)
		{
			_logger.NavigatingToUri(absoluteUri);
			using var nsUrl = new NSUrl(absoluteUri.ToString());
			using var request = new NSUrlRequest(nsUrl);
			_webview.LoadRequest(request);
		}

		internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers)
		{
			var defaultResult = TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);
			var hotReloadedResult = StaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, uri, ref statusCode, ref content, headers);
			return defaultResult || hotReloadedResult;
		}

		/// <inheritdoc />
		protected override void SendMessage(string message)
		{
			var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(message);
			_webview.EvaluateJavaScript(
				javascript: $"__dispatchMessageCallback(\"{messageJSStringLiteral}\")",
				completionHandler: (NSObject? result, NSError? error) => { });
		}

		internal void MessageReceivedInternal(Uri uri, string message)
		{
			MessageReceived(uri, message);
		}

		private void InitializeWebView()
		{
			_webview.NavigationDelegate = new WebViewNavigationDelegate(_blazorMauiWebViewHandler);
			_webview.UIDelegate = new WebViewUIDelegate(_blazorMauiWebViewHandler);
		}

		internal sealed class WebViewUIDelegate : WKUIDelegate
		{
			private static readonly string LocalOK = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("OK");
			private static readonly string LocalCancel = NSBundle.FromIdentifier("com.apple.UIKit").GetLocalizedString("Cancel");
			private readonly BlazorWebViewHandler _webView;

			public WebViewUIDelegate(BlazorWebViewHandler webView)
			{
				_webView = webView ?? throw new ArgumentNullException(nameof(webView));
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

			// TODO: this should be an override but requires XAMCORE_5_0: https://github.com/dotnet/macios/issues/15728
			[Export("webView:runJavaScriptTextInputPanelWithPrompt:defaultText:initiatedByFrame:completionHandler:")]
			public void RunJavaScriptTextInputPanelWithPrompt(
				WKWebView webView,
				string prompt,
				string? defaultText,
				WKFrameInfo frame,
				IntPtr completionHandlerBlock)
			{
				var completionHandler = ActionStringTrampolineBlock.Create(completionHandlerBlock);
				PresentAlertController(
					webView,
					prompt,
					defaultText: defaultText,
					okAction: x => completionHandler?.Invoke(x.TextFields[0].Text),
					cancelAction: _ => completionHandler?.Invoke(null)
				);
			}

			private static string GetJsAlertTitle(WKWebView webView)
			{
				// Emulate the behavior of UIWebView dialogs.
				// The scheme and host are used unless local html content is what the webview is displaying,
				// in which case the bundle file name is used.
				if (webView.Url != null && webView.Url.AbsoluteString != $"file://{NSBundle.MainBundle.BundlePath}/")
					return $"{webView.Url.Scheme}://{webView.Url.Host}";

				return new NSString(NSBundle.MainBundle.BundlePath).LastPathComponent;
			}

			private static UIAlertAction AddOkAction(UIAlertController controller, Action handler)
			{
				var action = UIAlertAction.Create(LocalOK, UIAlertActionStyle.Default, (_) => handler());
				controller.AddAction(action);
				controller.PreferredAction = action;
				return action;
			}

			private static UIAlertAction AddCancelAction(UIAlertController controller, Action handler)
			{
				var action = UIAlertAction.Create(LocalCancel, UIAlertActionStyle.Cancel, (_) => handler());
				controller.AddAction(action);
				return action;
			}

			private static void PresentAlertController(
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

				GetTopViewController(UIApplication.SharedApplication.GetKeyWindow()?.RootViewController)?
					.PresentViewController(controller, true, null);
			}

			private static UIViewController? GetTopViewController(UIViewController? viewController)
			{
				if (viewController is UINavigationController navigationController)
					return GetTopViewController(navigationController.VisibleViewController);

				if (viewController is UITabBarController tabBarController)
					return GetTopViewController(tabBarController.SelectedViewController!);

				if (viewController?.PresentedViewController != null)
					return GetTopViewController(viewController.PresentedViewController);

				return viewController;
			}

			// TODO: Remove after XAMCORE_5_0 is live: 
			//       https://github.com/dotnet/macios/issues/15728
			//       https://github.com/dotnet/macios/pull/22199
			sealed class ActionStringTrampolineBlock : TrampolineBlockBase
			{
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
				[UserDelegateType(typeof(Action<string?>))]
				delegate void Invoker(IntPtr block, NativeHandle obj);

				Invoker invoker;

				public unsafe ActionStringTrampolineBlock(BlockLiteral* block)
					: base(block)
				{
					invoker = block->GetDelegateForBlock<Invoker>();
				}

				[Preserve(Conditional = true)]
				public unsafe static Action<string?>? Create(IntPtr block)
				{
					if (block == IntPtr.Zero)
					{
						return null;
					}

					var del = (Action<string?>)GetExistingManagedDelegate(block);
					return del ?? new ActionStringTrampolineBlock((BlockLiteral*)block).Invoke;
				}

				void Invoke(string? obj)
				{
					var nsobj = CFString.CreateNative(obj);
					invoker(BlockPointer, nsobj);
					CFString.ReleaseNative(nsobj);
				}
			}
		}

		internal class WebViewNavigationDelegate : WKNavigationDelegate
		{
			private readonly BlazorWebViewHandler _webView;

			private WKNavigation? _currentNavigation;
			private Uri? _currentUri;

			public WebViewNavigationDelegate(BlazorWebViewHandler webView)
			{
				_webView = webView ?? throw new ArgumentNullException(nameof(webView));
			}

			public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
			{
				_currentNavigation = navigation;
			}

			public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
			{
				var requestUrl = navigationAction.Request.Url;
				var uri = new Uri(requestUrl.ToString());

				UrlLoadingStrategy strategy;

				// TargetFrame is null for navigation to a new window (`_blank`)
				if (navigationAction.TargetFrame is null)
				{
					// Open in a new browser window regardless of UrlLoadingStrategy
					strategy = UrlLoadingStrategy.OpenExternally;
				}
				else
				{
					// Invoke the UrlLoading event to allow overriding the default link handling behavior
					var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, BlazorWebViewHandler.AppOriginUri);
					_webView.UrlLoading(callbackArgs);
					_webView.Logger.NavigationEvent(uri, callbackArgs.UrlLoadingStrategy);

					strategy = callbackArgs.UrlLoadingStrategy;
				}

				if (strategy == UrlLoadingStrategy.OpenExternally)
				{
					_webView.Logger.LaunchExternalBrowser(uri);

					UIApplication.SharedApplication.OpenUrl(requestUrl, new UIApplicationOpenUrlOptions(), (success) =>
					{
						if (!success)
						{
							_webView.Logger.LogError($"There was an error trying to open URL: {requestUrl}");
						}
					});
				}

				if (strategy != UrlLoadingStrategy.OpenInWebView)
				{
					// Cancel any further navigation as we've either opened the link in the external browser
					// or canceled the underlying navigation action.
					decisionHandler(WKNavigationActionPolicy.Cancel);
					return;
				}

				if (navigationAction.TargetFrame!.MainFrame)
				{
					_currentUri = requestUrl;
				}

				decisionHandler(WKNavigationActionPolicy.Allow);
			}

			public override void DidReceiveServerRedirectForProvisionalNavigation(WKWebView webView, WKNavigation navigation)
			{
				// We need to intercept the redirects to the app scheme because Safari will block them.
				// We will handle these redirects through the Navigation Manager.
				if (_currentUri?.Host == BlazorWebView.AppHostAddress)
				{
					var uri = _currentUri;
					_currentUri = null;
					_currentNavigation = null;
					if (uri is not null)
					{
						var request = new NSUrlRequest(new NSUrl(uri.AbsoluteUri));
						webView.LoadRequest(request);
					}
				}
			}

			public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				_currentUri = null;
				_currentNavigation = null;
			}

			public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
			{
				_currentUri = null;
				_currentNavigation = null;
			}

			public override void DidCommitNavigation(WKWebView webView, WKNavigation navigation)
			{
				if (_currentUri != null && _currentNavigation == navigation)
				{
					// TODO: Determine whether this is needed
					//_webView.HandleNavigationStarting(_currentUri);
				}
			}

			public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
			{
				if (_currentUri != null && _currentNavigation == navigation)
				{
					// TODO: Determine whether this is needed
					//_webView.HandleNavigationFinished(_currentUri);
					_currentUri = null;
					_currentNavigation = null;
				}
			}
		}
	}
}
