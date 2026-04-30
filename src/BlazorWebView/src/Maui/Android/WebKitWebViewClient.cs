using System;
using System.Runtime.Versioning;
using Android.Content;
using Android.Runtime;
using Android.Webkit;
using Java.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	[SupportedOSPlatform("android23.0")]
	internal class WebKitWebViewClient : WebViewClient
	{
		// Using an IP address means that WebView doesn't wait for any DNS resolution,
		// making it substantially faster. Note that this isn't real HTTP traffic, since
		// we intercept all the requests within this origin.
		private static readonly string AppOrigin = $"https://{BlazorWebView.AppHostAddress}/";

		private static readonly Uri AppOriginUri = new(AppOrigin);

		// Single startup script that:
		//  - Is idempotent (guarded by window.__BlazorStarting) so duplicate OnPageFinished calls are safe.
		//  - Sets up window.external.sendMessage/receiveMessage for the Blazor interop bridge.
		//  - Listens for the native 'capturePort' message that delivers the native WebMessagePort.
		//  - Calls Blazor.start() only after the native port is captured, ensuring window.external.sendMessage
		//    has a live port before Blazor sends any messages.
		//  - Sets window.__BlazorStarted after the Blazor.start() Promise resolves (used by test helpers as a readiness signal).
		//  - Dispatches native→JS messages (arriving via PostWebMessage) directly to window.external.__callback.
		//  - Validates message origin: only processes messages from the native PostWebMessage API
		//    (event.source === null), skipping messages from subframes or other JS contexts.
		private const string BlazorInitScript = """
			(function () {
				if (window.__BlazorStarting) { return 'false'; }
				window.__BlazorStarting = true;

				window.external = window.external || {};
				window.external.sendMessage = function (message) {
					if (window.__nativePort) {
						window.__nativePort.postMessage(message);
					}
				};
				window.external.receiveMessage = function (callback) {
					window.external.__callback = callback;
				};

				window.addEventListener('message', function (event) {
					// Only process messages from the native PostWebMessage API.
					// Native messages have event.source === null; messages from subframes
					// or other JS contexts have event.source set to the sending window.
					if (event.source !== null) { return; }

					if (event.data === 'capturePort') {
						if (event.ports && event.ports[0] && !window.__nativePort) {
							window.__nativePort = event.ports[0];
							Promise.resolve(Blazor.start()).then(function () {
								window.__BlazorStarted = true;
							});
						}
					} else if (window.external.__callback) {
						window.external.__callback(event.data);
					}
				}, false);

				return 'true';
			})();
			""";

		private readonly BlazorWebViewHandler? _webViewHandler;

		public WebKitWebViewClient(BlazorWebViewHandler webViewHandler)
		{
			ArgumentNullException.ThrowIfNull(webViewHandler);
			_webViewHandler = webViewHandler;
		}

		protected WebKitWebViewClient(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			// This constructor is called whenever the .NET proxy was disposed, and it was recreated by Java. It also
			// happens when overridden methods are called between execution of this constructor and the one above.
			// because of these facts, we have to check all methods below for null field references and properties.
		}

		public override bool ShouldOverrideUrlLoading(AWebView? view, IWebResourceRequest? request)
#pragma warning disable CA1416 // TODO: base.ShouldOverrideUrlLoading(,) is supported from Android 24.0
			=> ShouldOverrideUrlLoadingCore(request) || base.ShouldOverrideUrlLoading(view, request);
#pragma warning restore CA1416

		private bool ShouldOverrideUrlLoadingCore(IWebResourceRequest? request)
		{
			if (_webViewHandler is null || !Uri.TryCreate(request?.Url?.ToString(), UriKind.RelativeOrAbsolute, out var uri))
			{
				return false;
			}

			// This method never gets called for navigation to a new window ('_blank'),
			// so we know we can safely invoke the UrlLoading event.
			var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, AppOriginUri);
			_webViewHandler.UrlLoading(callbackArgs);
			_webViewHandler.Logger.NavigationEvent(uri, callbackArgs.UrlLoadingStrategy);

			if (callbackArgs.UrlLoadingStrategy == UrlLoadingStrategy.OpenExternally)
			{
				_webViewHandler.Logger.LaunchExternalBrowser(uri);
				try
				{
					var intent = Intent.ParseUri(uri.OriginalString, IntentUriType.Scheme);
					_webViewHandler.Context.StartActivity(intent);
				}
				catch (URISyntaxException)
				{
					// This can occur if there is a problem with the URI formatting given its specified scheme.
					// Other platforms will silently ignore formatting issues, so we do the same here.
				}
				catch (ActivityNotFoundException)
				{
					// Do nothing if there is no activity to handle the intent. This is consistent with the
					// behavior on other platforms when a URL with an unknown scheme is clicked.
				}
				return true;
			}

			return callbackArgs.UrlLoadingStrategy != UrlLoadingStrategy.OpenInWebView;
		}

		public override WebResourceResponse? ShouldInterceptRequest(AWebView? view, IWebResourceRequest? request)
		{
			if (request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestUri = request?.Url?.ToString();

			var logger = _webViewHandler?.Logger;

			logger?.LogDebug("Intercepting request for {Url}.", requestUri);

			if (view is not null && request is not null && !string.IsNullOrEmpty(requestUri))
			{
				// 1. Check if the app wants to modify or override the request
				var response = WebRequestInterceptingWebView.TryInterceptResponseStream(_webViewHandler, view, request, requestUri, logger);
				if (response is not null)
				{
					return response;
				}

				// 2. Check if the request is for a Blazor resource
				response = GetResponse(requestUri, _webViewHandler?.Logger);
				if (response is not null)
				{
					return response;
				}
			}

			// 3. Otherwise, we let the request go through as is
			logger?.LogDebug("Request for {Url} was not handled.", requestUri);

			return base.ShouldInterceptRequest(view, request);
		}

		private WebResourceResponse? GetResponse(string requestUri, ILogger? logger)
		{
			var allowFallbackOnHostPage = AppOriginUri.IsBaseOfPage(requestUri);
			requestUri = QueryStringHelper.RemovePossibleQueryString(requestUri);

			logger?.HandlingWebRequest(requestUri);

			if (requestUri != null &&
				_webViewHandler != null &&
				_webViewHandler.WebviewManager != null &&
				_webViewHandler.WebviewManager.TryGetResponseContentInternal(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
			{
				var contentType = headers["Content-Type"];

				logger?.ResponseContentBeingSent(requestUri, statusCode);

				return new WebResourceResponse(contentType, "UTF-8", statusCode, statusMessage, headers, content);
			}
			else
			{
				logger?.ResponseContentNotFound(requestUri ?? string.Empty);
			}

			return null;
		}

		public override void OnPageFinished(AWebView? view, string? url)
		{
			base.OnPageFinished(view, url);

			if (view != null && url != null && AppOriginUri.IsBaseOfPage(url))
			{
				// Startup scripts must run in OnPageFinished. If scripts are run earlier they will have no lasting
				// effect because once the page content loads all the document state gets reset.
				RunBlazorStartupScripts(view);
			}
		}

		private void RunBlazorStartupScripts(AWebView view)
		{
			_webViewHandler?.Logger.RunningBlazorStartupScripts();

			view.EvaluateJavascript(BlazorInitScript, new JavaScriptValueCallback(result =>
			{
				// The init script returns 'true' if it performed first-time setup, or
				// 'false' if it was a no-op (duplicate OnPageFinished). Only create the
				// native MessageChannel when setup actually ran.
				if (result?.ToString() == "\"true\"")
				{
					_webViewHandler?.WebviewManager?.SetUpMessageChannel();
				}
				_webViewHandler?.Logger.BlazorStartupScriptsFinished();
			}));
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				//_webViewManager = null;
			}
		}

		private class JavaScriptValueCallback : Java.Lang.Object, IValueCallback
		{
			private readonly Action<Java.Lang.Object?> _callback;

			public JavaScriptValueCallback(Action<Java.Lang.Object?> callback)
			{
				ArgumentNullException.ThrowIfNull(callback);
				_callback = callback;
			}

			public void OnReceiveValue(Java.Lang.Object? value)
			{
				_callback(value);
			}
		}
	}
}
