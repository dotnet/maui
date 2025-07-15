using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Web;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
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

			// Invoke the WebViewInitializing event to allow custom configuration of the web view
			var initializingArgs = new WebViewInitializationStartedEventArgs(config);
			VirtualView?.WebViewInitializationStarted(initializingArgs);

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

			// Invoke the WebViewInitialized event to signal that the web view has been initialized
			var initializedArgs = new WebViewInitializationCompletedEventArgs(webview, config);
			VirtualView?.WebViewInitializationCompleted(initializedArgs);

			return webview;
		}

		internal static void EvaluateJavaScript(IHybridWebViewHandler handler, IHybridWebView hybridWebView, EvaluateJavaScriptAsyncRequest request)
		{
			handler.PlatformView.EvaluateJavaScript(request);
		}

		internal static void MapFlowDirection(IHybridWebViewHandler handler, IHybridWebView hybridWebView)
		{
			var scrollView = handler.PlatformView?.ScrollView;
			if (scrollView == null)
				return;
				
			scrollView.UpdateFlowDirection(hybridWebView);

			// On macOS, we need to refresh the scroll indicators when flow direction changes
			// But only for runtime changes, not during initial load
			if (OperatingSystem.IsMacCatalyst() && handler.PlatformView != null && handler.PlatformView.IsLoaded())
			{
				bool showsVertical = scrollView.ShowsVerticalScrollIndicator;
				bool showsHorizontal = scrollView.ShowsHorizontalScrollIndicator;

				scrollView.ShowsVerticalScrollIndicator = false;
				scrollView.ShowsHorizontalScrollIndicator = false;

				scrollView.ShowsVerticalScrollIndicator = showsVertical;
				scrollView.ShowsHorizontalScrollIndicator = showsHorizontal;
			}
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


		[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
		[RequiresDynamicCode(DynamicFeatures)]
#endif
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

		[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
		[RequiresDynamicCode(DynamicFeatures)]
#endif
		private class SchemeHandler : NSObject, IWKUrlSchemeHandler
		{
			private readonly WeakReference<HybridWebViewHandler?> _webViewHandler;

			public SchemeHandler(HybridWebViewHandler webViewHandler)
			{
				_webViewHandler = new(webViewHandler);
			}

			private HybridWebViewHandler? Handler => _webViewHandler is not null && _webViewHandler.TryGetTarget(out var h) ? h : null;

			// The `async void` is intentional here, as this is an event handler that represents the start
			// of a request for some data from the webview. Once the task is complete, the `IWKUrlSchemeTask`
			// object is used to send the response back to the webview.
			[Export("webView:startURLSchemeTask:")]
			[SupportedOSPlatform("ios11.0")]
			public async void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
				if (Handler is null || Handler is IViewHandler ivh && ivh.VirtualView is null)
				{
					return;
				}

				var url = urlSchemeTask.Request.Url.AbsoluteString;
				if (string.IsNullOrEmpty(url))
				{
					return;
				}

				var logger = Handler.MauiContext?.CreateLogger<HybridWebViewHandler>();

				logger?.LogDebug("Intercepting request for {Url}.", url);

				// 1. First check if the app wants to modify or override the request.
				if (WebRequestInterceptingWebView.TryInterceptResponseStream(Handler, webView, urlSchemeTask, url, logger))
				{
					return;
				}

				// 2. If this is an app request, then assume the request is for a local resource.
				if (new Uri(url) is Uri uri && AppOriginUri.IsBaseOf(uri))
				{
					logger?.LogDebug("Request for {Url} will be handled by .NET MAUI.", url);

					// 2.a. Check if the request is for a local resource
					var (bytes, contentType, statusCode) = await GetResponseBytesAsync(url, urlSchemeTask.Request, logger);

					// 2.b. Return the response header
					using var dic = new NSMutableDictionary<NSString, NSString>();
					if (contentType is not null)
					{
						dic[(NSString)"Content-Type"] = (NSString)contentType;
					}
					if (bytes?.Length > 0)
					{
						// Disable local caching which would otherwise prevent user scripts from executing correctly.
						dic[(NSString)"Cache-Control"] = (NSString)"no-cache, max-age=0, must-revalidate, no-store";
						dic[(NSString)"Content-Length"] = (NSString)bytes.Length.ToString(CultureInfo.InvariantCulture);
					}

					using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, statusCode, "HTTP/1.1", dic);
					urlSchemeTask.DidReceiveResponse(response);

					// 2.c. Return the body
					if (bytes?.Length > 0)
					{
						urlSchemeTask.DidReceiveData(bytes);
					}

					// 2.d. Finish the task
					urlSchemeTask.DidFinish();
				}

				// 3. If the request is not handled by the app nor is it a local source, then we let the WKWebView
				//    handle the request as it would normally do. This means that it will try to load the resource
				//    from the internet or from the local cache.

				logger?.LogDebug("Request for {Url} was not handled.", url);
			}

			private async Task<(NSData? ResponseBytes, string? ContentType, int StatusCode)> GetResponseBytesAsync(string url, NSUrlRequest request, ILogger? logger)
			{
				if (Handler is null)
				{
					return (null, ContentType: null, StatusCode: 404);
				}

				url = WebUtils.RemovePossibleQueryString(url);

				if (new Uri(url) is Uri uri && AppOriginUri.IsBaseOf(uri))
				{
					var relativePath = AppOriginUri.MakeRelativeUri(uri).ToString();

					var bundleRootDir = Path.Combine(NSBundle.MainBundle.ResourcePath, Handler.VirtualView.HybridRoot!);

					// 1.a. Try the special "_framework/hybridwebview.js" path
					if (relativePath == HybridWebViewDotJsPath)
					{
						logger?.LogDebug("Request for {Url} will return the hybrid web view script.", url);
						var jsStream = GetEmbeddedStream(HybridWebViewDotJsPath);
						if (jsStream is not null)
						{
							return (NSData.FromStream(jsStream), ContentType: "application/javascript", StatusCode: 200);
						}
					}

					// 1.b. Try special InvokeDotNet path
					if (relativePath == InvokeDotNetPath)
					{
						logger?.LogDebug("Request for {Url} will be handled by the .NET method invoker.", url);

						// Only accept requests that have the expected headers
						if (!HasExpectedHeaders(request.Headers))
						{
							logger?.LogError("InvokeDotNet endpoint missing or invalid request header");
							return (null, null, StatusCode: 400);
						}

						// Only accept POST requests
						if (!string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
						{
							logger?.LogError("InvokeDotNet endpoint only accepts POST requests. Received: {Method}", request.HttpMethod);
							return (null, null, StatusCode: 405);
						}

						// Read the request body
						Stream requestBody;
						if (request.Body is NSData bodyData && bodyData.Length > 0)
						{
							requestBody = bodyData.AsStream();
						}
						else
						{
							logger?.LogError("InvokeDotNet request body is empty");
							return (null, null, StatusCode: 400);
						}

						// Invoke the method
						var contentBytes = await Handler.InvokeDotNetAsync(streamBody: requestBody);
						if (contentBytes is not null)
						{
							return (NSData.FromArray(contentBytes), "application/json", StatusCode: 200);
						}
					}

					string contentType;

					// 2. If nothing found yet, try to get static content from the asset path
					if (string.IsNullOrEmpty(relativePath))
					{
						relativePath = Handler.VirtualView.DefaultFile;
						contentType = "text/html";
					}
					else
					{
						if (!ContentTypeProvider.TryGetContentType(relativePath, out contentType!))
						{
							contentType = "text/plain";
							logger?.LogWarning("Could not determine content type for '{relativePath}'", relativePath);
						}
					}

					var assetPath = Path.Combine(bundleRootDir, relativePath!);
					assetPath = FileSystemUtils.NormalizePath(assetPath);

					if (File.Exists(assetPath))
					{
						// 2.a. If something was found, return the content
						logger?.LogDebug("Request for {Url} will return an app package file.", url);

						return (NSData.FromFile(assetPath), contentType, StatusCode: 200);
					}
				}

				// 2.b. Otherwise, return a 404
				logger?.LogDebug("Request for {Url} could not be fulfilled.", url);
				return (null, ContentType: null, StatusCode: 404);
			}

			[Export("webView:stopURLSchemeTask:")]
			public void StopUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
			}
		}
	}
}
