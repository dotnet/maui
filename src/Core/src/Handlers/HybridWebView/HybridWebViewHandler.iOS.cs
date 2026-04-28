using System;
using System.Collections.Generic;
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
			// Update the WKWebView itself so SemanticContentAttribute is set correctly
			handler.PlatformView?.UpdateFlowDirection(hybridWebView);

			// Also update the internal ScrollView so the scrollbar aligns with the flow direction
			var scrollView = handler.PlatformView?.ScrollView;
			if (scrollView == null)
				return;

			scrollView.UpdateFlowDirectionForScrollView(hybridWebView);
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
			// Per Apple/WebKit docs, calling any completion method (DidReceiveResponse,
			// DidReceiveData, DidFinish, DidFailWithError) on a task after WKWebView has
			// invoked StopUrlSchemeTask throws NSInternalInconsistencyException, which
			// would crash the process from this `async void` method. We track stopped
			// tasks here and check before every completion call. The native handle is
			// used as the key to avoid keeping the task object alive longer than needed.
			private static readonly NSString HybridWebViewErrorDomain = new NSString("com.microsoft.maui.hybridwebview");

			private readonly WeakReference<HybridWebViewHandler?> _webViewHandler;
			private readonly HashSet<IntPtr> _stoppedTasks = new();
			private readonly object _stoppedTasksLock = new();

			public SchemeHandler(HybridWebViewHandler webViewHandler)
			{
				_webViewHandler = new(webViewHandler);
			}

			private HybridWebViewHandler? Handler => _webViewHandler is not null && _webViewHandler.TryGetTarget(out var h) ? h : null;

			private static IntPtr GetTaskHandle(IWKUrlSchemeTask urlSchemeTask) =>
				((NSObject)urlSchemeTask).Handle;

			private bool IsTaskStopped(IWKUrlSchemeTask urlSchemeTask)
			{
				var handle = GetTaskHandle(urlSchemeTask);
				lock (_stoppedTasksLock)
				{
					return _stoppedTasks.Contains(handle);
				}
			}

			private void ForgetTask(IWKUrlSchemeTask urlSchemeTask)
			{
				var handle = GetTaskHandle(urlSchemeTask);
				lock (_stoppedTasksLock)
				{
					_stoppedTasks.Remove(handle);
				}
			}

			// The `async void` is intentional here, as this is an event handler that represents the start
			// of a request for some data from the webview. Once the task is complete, the `IWKUrlSchemeTask`
			// object is used to send the response back to the webview.
			[Export("webView:startURLSchemeTask:")]
			[SupportedOSPlatform("ios11.0")]
			public async void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
				ILogger? logger = null;
				try
				{
					// If the handler/virtual view is gone, the task is being torn down. WebKit will (or
					// already has) call StopUrlSchemeTask, so any completion call here would throw.
					// Returning without completion is safe in this teardown scenario.
					if (Handler is null || Handler is IViewHandler ivh && ivh.VirtualView is null)
					{
						return;
					}

					logger = Handler.MauiContext?.CreateLogger<HybridWebViewHandler>();

					var url = urlSchemeTask.Request.Url?.AbsoluteString;
					if (string.IsNullOrEmpty(url))
					{
						logger?.LogDebug("Received URL scheme task with empty URL; failing the request.");
						SafeFailTask(urlSchemeTask, "The request URL was empty.");
						return;
					}

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

						// The await above is a yield point: WebKit may have called StopUrlSchemeTask
						// while we were loading the resource. If so, the task object is no longer
						// valid and any completion call will throw.
						if (IsTaskStopped(urlSchemeTask))
						{
							logger?.LogDebug("URL scheme task for {Url} was stopped before the response could be sent.", url);
							ForgetTask(urlSchemeTask);
							return;
						}

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

						using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url!, statusCode, "HTTP/1.1", dic);
						SafeInvoke(urlSchemeTask, t => t.DidReceiveResponse(response));

						// 2.c. Return the body
						if (bytes?.Length > 0)
						{
							SafeInvoke(urlSchemeTask, t => t.DidReceiveData(bytes));
						}

						// 2.d. Finish the task and return immediately so no later code can
						//      try to complete the task again.
						SafeInvoke(urlSchemeTask, t => t.DidFinish());
						return;
					}

					// 3. The 'app' scheme is registered exclusively to this handler — WebKit will not
					//    fall back to any other loader. If we don't complete the task here it will hang,
					//    so fail it explicitly.
					logger?.LogDebug("Request for {Url} was not handled by the app; failing the URL scheme task.", url);
					SafeFailTask(urlSchemeTask, $"Request for '{url}' was not handled.");
				}
				catch (Exception ex)
				{
					logger?.LogError(ex, "Unhandled exception while servicing URL scheme task.");
					SafeFailTask(urlSchemeTask, ex.Message);
				}
				finally
				{
					ForgetTask(urlSchemeTask);
				}
			}

			private void SafeInvoke(IWKUrlSchemeTask urlSchemeTask, Action<IWKUrlSchemeTask> action)
			{
				if (IsTaskStopped(urlSchemeTask))
				{
					return;
				}

				try
				{
					action(urlSchemeTask);
				}
				catch (Exception)
				{
					// The task was stopped by WKWebView between our check and the call (race),
					// or the native side rejected the call. Either way we must swallow the
					// exception — we are inside an `async void` and any throw crashes the app.
				}
			}

			private void SafeFailTask(IWKUrlSchemeTask urlSchemeTask, string message)
			{
				SafeInvoke(urlSchemeTask, t =>
				{
					using var userInfo = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
						[new NSString(message ?? string.Empty)],
						[NSError.LocalizedDescriptionKey]);
					using var error = new NSError(HybridWebViewErrorDomain, 500, userInfo);
					t.DidFailWithError(error);
				});
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
				// WebKit is telling us it no longer cares about this task. After this call,
				// any completion method invoked on the task will throw — record it so the
				// in-flight StartUrlSchemeTask handler can bail out cleanly.
				var handle = GetTaskHandle(urlSchemeTask);
				lock (_stoppedTasksLock)
				{
					_stoppedTasks.Add(handle);
				}
			}
		}
	}
}
