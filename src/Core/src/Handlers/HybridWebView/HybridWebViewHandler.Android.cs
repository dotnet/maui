using System;
using Android.Views;
using Android.Webkit;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, AWebView>
	{
		/// <inheritdoc/>
		public override bool NeedsContainer => true;

		/// <inheritdoc/>
		protected override void SetupContainer()
		{
			base.SetupContainer();

			// Use native child clipping, including HWUI's off-screen rejection,
			// rather than an explicit WebView ClipBounds rectangle (#38080).
			if (ContainerView is ViewGroup container)
				container.SetClipChildren(true);
		}

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

			// JS -> .NET messages flow through the SendMessagePath HTTP endpoint in
			// MauiHybridWebViewClient (gated by HasExpectedHeaders), not AddJavascriptInterface.

			// Invoke the WebViewInitializing event to allow custom configuration of the web view
			var initializingArgs = new WebViewInitializationStartedEventArgs(platformView.Settings);
			VirtualView.WebViewInitializationStarted(initializingArgs);

			// Invoke the WebViewInitialized event to signal that the web view has been initialized
			var initializedArgs = new WebViewInitializationCompletedEventArgs(platformView, platformView.Settings);
			VirtualView?.WebViewInitializationCompleted(initializedArgs);

			return platformView;
		}

		protected override void ConnectHandler(AWebView platformView)
		{
			base.ConnectHandler(platformView);

			var webViewClient = new MauiHybridWebViewClient(this);
			PlatformView.SetWebViewClient(webViewClient);

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
				//if (platformView.WebChromeClient is MauiWebChromeClient webChromeClient)
				//{
				//	webChromeClient.Disconnect();
				//}
			}

			platformView.SetWebViewClient(null!);
			//platformView.SetWebChromeClient(null);

			platformView.StopLoading();
			if (ContainerView?.Parent is ViewGroup containerParent)
				containerParent.RemoveView(ContainerView);
			HasContainer = false;

			base.DisconnectHandler(platformView);
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
