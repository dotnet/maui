using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Web;
using Foundation;
using UIKit;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, WKWebView>
	{
		private const string ScriptMessageHandlerName = "webwindowinterop";

		protected override WKWebView CreatePlatformView()
		{
			var config = new WKWebViewConfiguration();

			// By default, setting inline media playback to allowed, including autoplay
			// and picture in picture, since these things MUST be set during the webview
			// creation, and have no effect if set afterwards.
			// A custom handler factory delegate could be set to disable these defaults
			// but if we do not set them here, they cannot be changed once the
			// handler's platform view is created, so erring on the side of wanting this
			// capability by default.
			if (OperatingSystem.IsMacCatalystVersionAtLeast(10) || OperatingSystem.IsIOSVersionAtLeast(10))
			{
				config.AllowsPictureInPictureMediaPlayback = true;
				config.AllowsInlineMediaPlayback = true;
				config.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
			}

			config.DefaultWebpagePreferences!.AllowsContentJavaScript = true;

			config.UserContentController.AddScriptMessageHandler(new WebViewScriptMessageHandler(this), ScriptMessageHandlerName);
			// iOS WKWebView doesn't allow handling 'http'/'https' schemes, so we use the fake 'app' scheme
			config.SetUrlSchemeHandler(new SchemeHandler(this), urlScheme: "app");

			var webview = new MauiHybridWebView(this, RectangleF.Empty, config)
			{
				BackgroundColor = UIColor.Clear,
				AutosizesSubviews = true
			};

			if (DeveloperTools.Enabled)
			{
				// Legacy Developer Extras setting.
				config.Preferences.SetValueForKey(NSObject.FromObject(true), new NSString("developerExtrasEnabled"));

				if (OperatingSystem.IsIOSVersionAtLeast(16, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(16, 6))
				{
					// Enable Developer Extras for iOS builds for 16.4+ and Mac Catalyst builds for 16.6 (macOS 13.5)+
					webview.SetValueForKey(NSObject.FromObject(true), new NSString("inspectable"));
				}
			}

			return webview;
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

		private void MessageReceived(Uri uri, string message)
		{
			MessageReceived(message);
		}

		protected override void ConnectHandler(WKWebView platformView)
		{
			base.ConnectHandler(platformView);

			using var nsUrl = new NSUrl(new Uri(AppOriginUri, "/").ToString());
			using var request = new NSUrlRequest(nsUrl);

			platformView.LoadRequest(request);
		}

		protected override void DisconnectHandler(WKWebView platformView)
		{
			platformView.Configuration.UserContentController.RemoveScriptMessageHandler(ScriptMessageHandlerName);

			base.DisconnectHandler(platformView);
		}

		private sealed class WebViewScriptMessageHandler : NSObject, IWKScriptMessageHandler
		{
			private readonly WeakReference<HybridWebViewHandler?> _webViewHandler;

			public WebViewScriptMessageHandler(HybridWebViewHandler webViewHandler)
			{
				_webViewHandler = new(webViewHandler);
			}

			private HybridWebViewHandler? Handler => _webViewHandler is not null && _webViewHandler.TryGetTarget(out var h) ? h : null;

			public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
			{
				ArgumentNullException.ThrowIfNull(message);
				Handler?.MessageReceived(AppOriginUri, ((NSString)message.Body).ToString());
			}
		}

		private class SchemeHandler : NSObject, IWKUrlSchemeHandler
		{
			private readonly WeakReference<HybridWebViewHandler?> _webViewHandler;

			public SchemeHandler(HybridWebViewHandler webViewHandler)
			{
				_webViewHandler = new(webViewHandler);
			}

			private HybridWebViewHandler? Handler => _webViewHandler is not null && _webViewHandler.TryGetTarget(out var h) ? h : null;

			[Export("webView:startURLSchemeTask:")]
			[SupportedOSPlatform("ios11.0")]
			public void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
				var url = urlSchemeTask.Request.Url?.AbsoluteString ?? "";

				var responseData = GetResponseBytes(url);

				if (responseData.StatusCode == 200)
				{
					using (var dic = new NSMutableDictionary<NSString, NSString>())
					{
						dic.Add((NSString)"Content-Length", (NSString)(responseData.ResponseBytes.Length.ToString(CultureInfo.InvariantCulture)));
						dic.Add((NSString)"Content-Type", (NSString)responseData.ContentType);
						// Disable local caching. This will prevent user scripts from executing correctly.
						dic.Add((NSString)"Cache-Control", (NSString)"no-cache, max-age=0, must-revalidate, no-store");
						if (urlSchemeTask.Request.Url != null)
						{
							using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, responseData.StatusCode, "HTTP/1.1", dic);
							urlSchemeTask.DidReceiveResponse(response);
						}
					}

					urlSchemeTask.DidReceiveData(NSData.FromArray(responseData.ResponseBytes));
					urlSchemeTask.DidFinish();
				}
			}

			private (byte[] ResponseBytes, string ContentType, int StatusCode) GetResponseBytes(string? url)
			{
				if (Handler is null)
				{
					return (Array.Empty<byte>(), ContentType: string.Empty, StatusCode: 404);
				}

				var fullUrl = url;
				url = HybridWebViewQueryStringHelper.RemovePossibleQueryString(url);

				if (new Uri(url) is Uri uri && AppOriginUri.IsBaseOf(uri))
				{
					var relativePath = AppOriginUri.MakeRelativeUri(uri).ToString().Replace('\\', '/');

					var bundleRootDir = Path.Combine(NSBundle.MainBundle.ResourcePath, Handler.VirtualView.HybridRoot!);

					// 1. Try special InvokeDotNet path
					if (relativePath == InvokeDotNetPath)
					{
						var fullUri = new Uri(fullUrl!);
						var invokeQueryString = HttpUtility.ParseQueryString(fullUri.Query);
						(var contentBytes, var bytesContentType) = Handler.InvokeDotNet(invokeQueryString);
						if (contentBytes is not null)
						{
							return (contentBytes, bytesContentType!, StatusCode: 200);
						}
					}

					string contentType;

					// 2. If nothing found yet, try to get static content from the asset path
					if (string.IsNullOrEmpty(relativePath))
					{
						relativePath = Handler.VirtualView.DefaultFile!.Replace('\\', '/');
						contentType = "text/html";
					}
					else
					{
						if (!ContentTypeProvider.TryGetContentType(relativePath, out contentType!))
						{
							// TODO: Log this
							contentType = "text/plain";
						}
					}

					var assetPath = Path.Combine(bundleRootDir, relativePath);

					if (File.Exists(assetPath))
					{
						return (File.ReadAllBytes(assetPath), contentType, StatusCode: 200);
					}
				}

				return (Array.Empty<byte>(), ContentType: string.Empty, StatusCode: 404);
			}

			[Export("webView:stopURLSchemeTask:")]
			public void StopUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
			}
		}
	}
}
