using System;
using Android.Webkit;
using Java.Interop;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, AWebView>
	{
		// This name matches the name of the API used in HybridWebView.js and must remain in sync
		private const string HybridWebViewHostJsName = "hybridWebViewHost";

		private HybridWebViewJavaScriptInterface? _javaScriptInterface;

		protected override AWebView CreatePlatformView()
		{
			var platformView = new MauiHybridWebView(this, Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
			};

			platformView.Settings.DomStorageEnabled = true;
			platformView.Settings.SetSupportMultipleWindows(true);

			// Note that this is a per-app setting and not per-control, so if you enable
			// this, it is enabled for all Android WebViews in the app.
			if (DeveloperTools.Enabled)
			{
				AWebView.SetWebContentsDebuggingEnabled(enabled: true);
			}

			platformView.Settings.JavaScriptEnabled = true;

			_javaScriptInterface = new HybridWebViewJavaScriptInterface(this);
			platformView.AddJavascriptInterface(_javaScriptInterface, HybridWebViewHostJsName);

			// Invoke the WebViewInitializing event to allow custom configuration of the web view
			var initializingArgs = new WebViewInitializationStartedEventArgs(platformView.Settings);
			VirtualView.WebViewInitializationStarted(initializingArgs);

			// Invoke the WebViewInitialized event to signal that the web view has been initialized
			var initializedArgs = new WebViewInitializationCompletedEventArgs(platformView, platformView.Settings);
			VirtualView?.WebViewInitializationCompleted(initializedArgs);

			return platformView;
		}

		private sealed class HybridWebViewJavaScriptInterface : HybridJavaScriptInterface
		{
			private readonly WeakReference<HybridWebViewHandler> _hybridWebViewHandler;

			public HybridWebViewJavaScriptInterface(HybridWebViewHandler hybridWebViewHandler)
			{
				_hybridWebViewHandler = new(hybridWebViewHandler);
			}

			private HybridWebViewHandler? Handler => _hybridWebViewHandler is not null && _hybridWebViewHandler.TryGetTarget(out var h) ? h : null;

			[JavascriptInterface]
			public override void SendMessage(string message)
			{
				Handler?.MessageReceived(message);
			}
		}

		protected override void ConnectHandler(AWebView platformView)
		{
			base.ConnectHandler(platformView);

			var webViewClient = new MauiHybridWebViewClient(this);
			PlatformView.SetWebViewClient(webViewClient);

			var chromeClient = new HybridWebViewChromeClient(this);
			PlatformView.SetWebChromeClient(chromeClient);

			platformView.LoadUrl(new Uri(AppOriginUri, "/").ToString());
		}

		protected override void DisconnectHandler(AWebView platformView)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				if (platformView.WebViewClient is MauiHybridWebViewClient webViewClient)
				{
					webViewClient.Disconnect();
				}
			}

			platformView.SetWebViewClient(null!);
			platformView.SetWebChromeClient(null);

			platformView.StopLoading();


			base.DisconnectHandler(platformView);
		}

		sealed class HybridWebViewChromeClient : WebChromeClient
		{
			readonly WeakReference<HybridWebViewHandler> _handler;

			public HybridWebViewChromeClient(HybridWebViewHandler handler)
			{
				_handler = new WeakReference<HybridWebViewHandler>(handler);
			}

			public override bool OnCreateWindow(AWebView? view, bool isDialog, bool isUserGesture, global::Android.OS.Message? resultMsg)
			{
				if (view is null || !_handler.TryGetTarget(out var handler))
					return false;

				var requestUrl = view.GetHitTestResult()?.Extra;
				if (string.IsNullOrEmpty(requestUrl))
					return false;

				Uri.TryCreate(requestUrl, UriKind.RelativeOrAbsolute, out var uri);

				if (handler.VirtualView is INavigatingAwareWebView navAware)
				{
					var args = new WebViewNavigatingEventArgs(uri, WebNavigationTarget.NewWindow, null);
					bool cancel = navAware.Navigating(args);

					if (!cancel)
					{
						view.LoadUrl(requestUrl);
					}
				}
				else
				{
					view.LoadUrl(requestUrl);
				}

				return false;
			}
		}

		internal static void EvaluateJavaScript(IHybridWebViewHandler handler, IHybridWebView hybridWebView, EvaluateJavaScriptAsyncRequest request)
		{
			handler.PlatformView.EvaluateJavaScript(request);
		}

		public static void MapSendRawMessage(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
			if (arg is not HybridWebViewRawMessage hybridWebViewRawMessage || handler.PlatformView is not IHybridPlatformWebView hybridPlatformWebView)
			{
				return;
			}

			hybridPlatformWebView.SendRawMessage(hybridWebViewRawMessage.Message ?? "");
		}
	}
}
