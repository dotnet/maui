using System;
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
				config.Preferences.SetValueForKey(NSObject.FromObject(true), new NSString("developerExtrasEnabled"));

				if (OperatingSystem.IsIOSVersionAtLeast(16, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(16, 6))
				{
					// Enable Developer Extras for iOS builds for 16.4+ and Mac Catalyst builds for 16.6 (macOS 13.5)+
					webview.SetValueForKey(NSObject.FromObject(true), new NSString("inspectable"));
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
				var responseBytes = GetResponseBytes(url, out var contentType, statusCode: out var statusCode);
				if (statusCode == 200)
				{
					using (var dic = new NSMutableDictionary<NSString, NSString>())
					{
						dic.Add((NSString)"Content-Length", (NSString)responseBytes.Length.ToString(CultureInfo.InvariantCulture));
						dic.Add((NSString)"Content-Type", (NSString)contentType);
						// Disable local caching. This will prevent user scripts from executing correctly.
						dic.Add((NSString)"Cache-Control", (NSString)"no-cache, max-age=0, must-revalidate, no-store");
						if (urlSchemeTask.Request.Url != null)
						{
							using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, statusCode, "HTTP/1.1", dic);
							urlSchemeTask.DidReceiveResponse(response);
						}

					}
					urlSchemeTask.DidReceiveData(NSData.FromArray(responseBytes));
					urlSchemeTask.DidFinish();
				}

				// 3. If the request is not handled by the app nor is it a local source, then we let the WKWebView
				//    handle the request as it would normally do. This means that it will try to load the resource
				//    from the internet or from the local cache.

				logger.LogDebug("Request for {Url} was not handled.", url);
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
