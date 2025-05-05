﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Web;
using Foundation;
using Microsoft.Extensions.Logging;
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

				var url = urlSchemeTask.Request.Url.AbsoluteString ?? "";

				var logger = Handler.MauiContext?.CreateLogger<HybridWebViewHandler>();

				logger?.LogDebug("Intercepting request for {Url}.", url);

				// 1. First check if the app wants to modify or override the request.
				{
					// 1.a. First, create the event args
					var platformArgs = new WebResourceRequestedEventArgs(webView, urlSchemeTask);

					// 1.b. Trigger the event for the app
					var handled = Handler.VirtualView.WebResourceRequested(platformArgs);

					// 1.c. If the app reported that it completed the request, then we do nothing more
					if (handled)
					{
						logger?.LogDebug("Request for {Url} was handled by the user.", url);

						return;
					}
				}

				// 2. If this is an app request, then assume the request is for a local resource.
				if (new Uri(url) is Uri uri && AppOriginUri.IsBaseOf(uri))
				{
					logger?.LogDebug("Request for {Url} will be handled by .NET MAUI.", url);

					// 2.a. Check if the request is for a local resource
					var (bytes, contentType, statusCode) = await GetResponseBytesAsync(url, logger);

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
						urlSchemeTask.DidReceiveData(NSData.FromArray(bytes));
					}

					// 2.d. Finish the task
					urlSchemeTask.DidFinish();
				}

				// 3. If the request is not handled by the app nor is it a local source, then we let the WKWebView
				//    handle the request as it would normally do. This means that it will try to load the resource
				//    from the internet or from the local cache.

				logger?.LogDebug("Request for {Url} was not handled.", url);
			}

			private async Task<(byte[]? ResponseBytes, string? ContentType, int StatusCode)> GetResponseBytesAsync(string? url, ILogger? logger)
			{
				if (Handler is null)
				{
					return (null, ContentType: null, StatusCode: 404);
				}

				var fullUrl = url;
				url = WebUtils.RemovePossibleQueryString(url);

				if (new Uri(url) is Uri uri && AppOriginUri.IsBaseOf(uri))
				{
					var relativePath = AppOriginUri.MakeRelativeUri(uri).ToString().Replace('\\', '/');

					var bundleRootDir = Path.Combine(NSBundle.MainBundle.ResourcePath, Handler.VirtualView.HybridRoot!);

					// 1. Try special InvokeDotNet path
					if (relativePath == InvokeDotNetPath)
					{
						logger?.LogDebug("Request for {Url} will be handled by the .NET method invoker.", url);

						var fullUri = new Uri(fullUrl!);
						var invokeQueryString = HttpUtility.ParseQueryString(fullUri.Query);
						var contentBytes = await Handler.InvokeDotNetAsync(invokeQueryString);
						if (contentBytes is not null)
						{
							return (contentBytes, "application/json", StatusCode: 200);
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
							contentType = "text/plain";
							logger?.LogWarning("Could not determine content type for '{relativePath}'", relativePath);
						}
					}

					var assetPath = Path.Combine(bundleRootDir, relativePath);

					if (File.Exists(assetPath))
					{
						// 2.a. If something was found, return the content
						logger?.LogDebug("Request for {Url} will return an app package file.", url);

						return (File.ReadAllBytes(assetPath), contentType, StatusCode: 200);
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
