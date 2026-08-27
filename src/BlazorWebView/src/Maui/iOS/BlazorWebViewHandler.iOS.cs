using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// The iOS and Mac Catalyst <see cref="ViewHandler"/> for <see cref="BlazorWebView"/>.
	/// </summary>
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WKWebView>
	{
		private IOSWebViewManager? _webviewManager;
		private readonly StaticContentResponseCache _staticContentResponseCache = new();

		internal static string AppOrigin { get; } = "app://" + BlazorWebView.AppHostAddress + "/";
		internal static Uri AppOriginUri { get; } = new(AppOrigin);
		private const string BlazorInitScript = @"
			window.__receiveMessageCallbacks = [];
			window.__dispatchMessageCallback = function(message) {
				window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
			};
			window.external = {
				sendMessage: function(message) {
					window.webkit.messageHandlers.webwindowinterop.postMessage(message);
				},
				receiveMessage: function(callback) {
					window.__receiveMessageCallbacks.push(callback);
				}
			};

			Blazor.start();

			(function () {
				window.onpageshow = function(event) {
					if (event.persisted) {
						window.location.reload();
					}
				};
			})();
		";

		private ILogger? _logger;
		internal ILogger Logger => _logger ??= Services!.GetService<ILogger<BlazorWebViewHandler>>() ?? NullLogger<BlazorWebViewHandler>.Instance;

		/// <inheritdoc />
		[SupportedOSPlatform("ios11.0")]
		protected override WKWebView CreatePlatformView()
		{
			Logger.CreatingWebKitWKWebView();

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

			VirtualView.BlazorWebViewInitializing(new BlazorWebViewInitializingEventArgs()
			{
				Configuration = config
			});

			config.UserContentController.AddScriptMessageHandler(new WebViewScriptMessageHandler(MessageReceived), "webwindowinterop");
			config.UserContentController.AddUserScript(new WKUserScript(
				new NSString(BlazorInitScript), WKUserScriptInjectionTime.AtDocumentEnd, true));

			// iOS WKWebView doesn't allow handling 'http'/'https' schemes, so we use the fake 'app' scheme
			config.SetUrlSchemeHandler(new SchemeHandler(this), urlScheme: "app");

			var webview = new WKWebView(RectangleF.Empty, config)
			{
				BackgroundColor = UIColor.Clear,
				AutosizesSubviews = true
			};

			if (DeveloperTools.Enabled)
			{
				// Legacy Developer Extras setting.
				if (NSObject.FromObject(true) is NSObject trueValue)
				{
					config.Preferences.SetValueForKey(trueValue, new NSString("developerExtrasEnabled"));

					if (OperatingSystem.IsIOSVersionAtLeast(16, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(16, 6))
					{
						// Enable Developer Extras for iOS builds for 16.4+ and Mac Catalyst builds for 16.6 (macOS 13.5)+
						webview.SetValueForKey(trueValue, new NSString("inspectable"));
					}
				}
			}

			VirtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs
			{
				WebView = webview
			});

			// Disable bounce scrolling to make Blazor apps feel more native
			if (webview.ScrollView != null)
			{
				webview.ScrollView.Bounces = false;
				webview.ScrollView.AlwaysBounceVertical = false;
				webview.ScrollView.AlwaysBounceHorizontal = false;
			}

			Logger.CreatedWebKitWKWebView();

			return webview;
		}

		private void MessageReceived(Uri uri, string message)
		{
			_webviewManager?.MessageReceivedInternal(uri, message);
		}

		/// <inheritdoc />
		protected override void DisconnectHandler(WKWebView platformView)
		{
			platformView.StopLoading();
			_staticContentResponseCache.Clear();

			if (_webviewManager != null)
			{
				// Start the disposal...
				var disposalTask = _webviewManager?
					.DisposeAsync()
					.AsTask()!;

				if (IsBlockingDisposalEnabled)
				{
					// If the app is configured to block on dispose via an AppContext switch,
					// we'll synchronously wait for the disposal to complete. This can cause a deadlock.
					disposalTask
						.GetAwaiter()
						.GetResult();
				}
				else
				{
					// Otherwise, by default, we'll fire-and-forget the disposal task.
					disposalTask.FireAndForget(_logger);
				}

				_webviewManager = null;
			}
		}

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				_webviewManager != null)
			{
				return;
			}
			if (PlatformView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			Logger.CreatingFileProvider(contentRootDir, hostPageRelativePath);

			var fileProvider = VirtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new IOSWebViewManager(
				this,
				PlatformView,
				Services!,
				new MauiDispatcher(Services!.GetRequiredService<IDispatcher>()),
				fileProvider,
				VirtualView.JSComponents,
				contentRootDir,
				hostPageRelativePath,
				Logger);

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

			if (RootComponents != null)
			{
				foreach (var rootComponent in RootComponents)
				{
					Logger.AddingRootComponent(rootComponent.ComponentType?.FullName ?? string.Empty, rootComponent.Selector ?? string.Empty, rootComponent.Parameters?.Count ?? 0);

					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}

			Logger.StartingInitialNavigation(VirtualView.StartPath);
			_webviewManager.Navigate(VirtualView.StartPath);
		}

		internal IFileProvider CreateFileProvider(string contentRootDir)
		{
			return new iOSMauiAssetFileProvider(contentRootDir);
		}

		/// <summary>
		/// Calls the specified <paramref name="workItem"/> asynchronously and passes in the scoped services available to Razor components.
		/// </summary>
		/// <param name="workItem">The action to call.</param>
		/// <returns>Returns a <see cref="Task"/> representing <c>true</c> if the <paramref name="workItem"/> was called, or <c>false</c> if it was not called because Blazor is not currently running.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="workItem"/> is <c>null</c>.</exception>
		public virtual async Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
		{
			ArgumentNullException.ThrowIfNull(workItem);
			if (_webviewManager is null)
			{
				return false;
			}

			return await _webviewManager.TryDispatchAsync(workItem);
		}

		private sealed class WebViewScriptMessageHandler : NSObject, IWKScriptMessageHandler
		{
			private Action<Uri, string> _messageReceivedAction;

			public WebViewScriptMessageHandler(Action<Uri, string> messageReceivedAction)
			{
				_messageReceivedAction = messageReceivedAction ?? throw new ArgumentNullException(nameof(messageReceivedAction));
			}

			public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
			{
				if (message is null)
				{
					throw new ArgumentNullException(nameof(message));
				}
				_messageReceivedAction(AppOriginUri, ((NSString)message.Body).ToString());
			}
		}

		private class SchemeHandler : NSObject, IWKUrlSchemeHandler
		{
			private readonly BlazorWebViewHandler _webViewHandler;

			public SchemeHandler(BlazorWebViewHandler webViewHandler)
			{
				_webViewHandler = webViewHandler;
			}

			[Export("webView:startURLSchemeTask:")]
			[SupportedOSPlatform("ios11.0")]
			public void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
				var url = urlSchemeTask.Request.Url.AbsoluteString;
				if (string.IsNullOrEmpty(url))
				{
					return;
				}

				var logger = _webViewHandler.Logger;

				logger.LogDebug("Intercepting request for {Url}.", url);

				// 1. First check if the app wants to modify or override the request.
				if (WebRequestInterceptingWebView.TryInterceptResponseStream(_webViewHandler, webView, urlSchemeTask, url, logger))
				{
					return;
				}

				// 2. If this is an app request, then assume the request is for a Blazor resource.
				StaticContentCacheRequestBehavior? cacheRequestBehavior = null;
				if (_webViewHandler._staticContentResponseCache.TryGet(url, out var cachedResponse))
				{
					cacheRequestBehavior = StaticContentResponseCachePolicy.GetRequestBehavior(
						urlSchemeTask.Request.HttpMethod,
						GetRequestHeaders(urlSchemeTask.Request));
					if (cacheRequestBehavior == StaticContentCacheRequestBehavior.Default)
					{
						var cachedRequestUri = QueryStringHelper.RemovePossibleQueryString(url);
						logger.HandlingWebRequest(cachedRequestUri);
						logger.ResponseContentBeingSent(cachedRequestUri, cachedResponse.StatusCode);
						SendResponse(urlSchemeTask, cachedResponse);
						return;
					}

					if (cacheRequestBehavior == StaticContentCacheRequestBehavior.Refresh)
					{
						_webViewHandler._staticContentResponseCache.Remove(url);
					}
				}

				var responseBytes = GetResponseBytes(url, out var contentType, statusCode: out var statusCode);
				if (statusCode == 200)
				{
					// By default local caching is disabled so that user scripts are always re-executed. Applications can
					// opt specific resources into caching via BlazorWebView.StaticContentCacheControlProvider.
					// The original (unstripped) URI is passed so the provider can act on query strings (e.g. img.png?v=2).
					// See https://github.com/dotnet/maui/issues/8279
					var cacheControl = StaticContentCacheControl.ResolveOverride(_webViewHandler.VirtualView, url, contentType, logger)
						?? StaticContentCacheControl.Default;

					var cacheLifetime = default(TimeSpan);
					var shouldCache = StaticContentResponseCachePolicy.TryGetCacheLifetime(cacheControl, out cacheLifetime);
					if (shouldCache)
					{
						cacheRequestBehavior ??= StaticContentResponseCachePolicy.GetRequestBehavior(
							urlSchemeTask.Request.HttpMethod,
							GetRequestHeaders(urlSchemeTask.Request));
						shouldCache = cacheRequestBehavior != StaticContentCacheRequestBehavior.Disabled;
					}

					if (shouldCache)
					{
						var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
						{
							["Content-Type"] = contentType,
							["Cache-Control"] = cacheControl,
						};
						var response = new StaticContentResponse(
							url,
							contentType,
							statusCode,
							"OK",
							headers,
							responseBytes,
							StaticContentResponseCachePolicy.GetExpiration(cacheLifetime));

						_webViewHandler._staticContentResponseCache.Set(response);
						SendResponse(urlSchemeTask, response);
					}
					else
					{
						SendResponse(urlSchemeTask, statusCode, contentType, cacheControl, responseBytes);
					}

					return;
				}

				// 3. If the request is not handled by the app nor is it a local source, then we let the WKWebView
				//    handle the request as it would normally do. This means that it will try to load the resource
				//    from the internet or from the local cache.

				logger.LogDebug("Request for {Url} was not handled.", url);
			}

			private static IEnumerable<KeyValuePair<string, string>> GetRequestHeaders(NSUrlRequest request)
			{
				var headers = request.Headers;
				if (headers is null)
				{
					yield break;
				}

				foreach (var key in headers.Keys)
				{
					if (key?.ToString() is string keyString &&
						headers[key]?.ToString() is string valueString)
					{
						yield return new KeyValuePair<string, string>(keyString, valueString);
					}
				}
			}

			private static void SendResponse(IWKUrlSchemeTask urlSchemeTask, StaticContentResponse response)
				=> SendResponse(
					urlSchemeTask,
					response.StatusCode,
					response.ContentType,
					response.Headers["Cache-Control"],
					response.Content);

			private static void SendResponse(
				IWKUrlSchemeTask urlSchemeTask,
				int statusCode,
				string contentType,
				string cacheControl,
				byte[] content)
			{
				using (var headers = new NSMutableDictionary<NSString, NSString>())
				{
					headers.Add((NSString)"Content-Length", (NSString)content.Length.ToString(CultureInfo.InvariantCulture));
					headers.Add((NSString)"Content-Type", (NSString)contentType);
					headers.Add((NSString)"Cache-Control", (NSString)cacheControl);
					if (urlSchemeTask.Request.Url != null)
					{
						using var urlResponse = new NSHttpUrlResponse(urlSchemeTask.Request.Url, statusCode, "HTTP/1.1", headers);
						urlSchemeTask.DidReceiveResponse(urlResponse);
					}
				}

				using var data = NSData.FromArray(content);
				urlSchemeTask.DidReceiveData(data);
				urlSchemeTask.DidFinish();
			}

			private byte[] GetResponseBytes(string? url, out string contentType, out int statusCode)
			{
				var allowFallbackOnHostPage = AppOriginUri.IsBaseOfPage(url);
				url = QueryStringHelper.RemovePossibleQueryString(url);

				_webViewHandler.Logger.HandlingWebRequest(url);

				if (_webViewHandler._webviewManager!.TryGetResponseContentInternal(url, allowFallbackOnHostPage, out statusCode, out var statusMessage, out var content, out var headers))
				{
					statusCode = 200;
					using var ms = new MemoryStream();

					content.CopyTo(ms);
					content.Dispose();

					contentType = headers["Content-Type"];

					_webViewHandler?.Logger.ResponseContentBeingSent(url, statusCode);

					return ms.ToArray();
				}
				else
				{
					_webViewHandler?.Logger.ResponseContentNotFound(url);

					statusCode = 404;
					contentType = string.Empty;
					return Array.Empty<byte>();
				}
			}

			[Export("webView:stopURLSchemeTask:")]
			public void StopUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
			}
		}
	}
}
