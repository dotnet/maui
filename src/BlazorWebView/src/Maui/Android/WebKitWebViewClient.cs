using System;
using System.Collections.Generic;
using System.IO;
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

		private readonly BlazorWebViewHandler? _webViewHandler;
		// Android does not store WebResourceResponse instances returned from ShouldInterceptRequest in Chromium's
		// HTTP cache. Keep explicitly cacheable static responses in a bounded per-WebView cache instead.
		private readonly StaticContentResponseCache _staticContentResponseCache = new();

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
				response = GetResponse(request, requestUri, _webViewHandler?.Logger);
				if (response is not null)
				{
					return response;
				}
			}

			// 3. Otherwise, we let the request go through as is
			logger?.LogDebug("Request for {Url} was not handled.", requestUri);

			return base.ShouldInterceptRequest(view, request);
		}

		private WebResourceResponse? GetResponse(IWebResourceRequest request, string requestUri, ILogger? logger)
		{
			if (!Uri.TryCreate(requestUri, UriKind.Absolute, out var uri) || !AppOriginUri.IsBaseOf(uri))
			{
				return null;
			}

			StaticContentCacheRequestBehavior? cacheRequestBehavior = null;
			if (_staticContentResponseCache.TryGet(requestUri, out var cachedResponse))
			{
				cacheRequestBehavior = StaticContentResponseCachePolicy.GetRequestBehavior(request.Method, request.RequestHeaders);
				if (cacheRequestBehavior == StaticContentCacheRequestBehavior.Default)
				{
					var cachedRequestUri = QueryStringHelper.RemovePossibleQueryString(requestUri);
					logger?.HandlingWebRequest(cachedRequestUri);
					logger?.ResponseContentBeingSent(cachedRequestUri, cachedResponse.StatusCode);
					return CreateWebResourceResponse(cachedResponse);
				}

				if (cacheRequestBehavior == StaticContentCacheRequestBehavior.Refresh)
				{
					_staticContentResponseCache.Remove(requestUri);
				}
			}

			var allowFallbackOnHostPage = AppOriginUri.IsBaseOfPage(requestUri);
			var originalRequestUri = requestUri;
			requestUri = QueryStringHelper.RemovePossibleQueryString(requestUri);

			logger?.HandlingWebRequest(requestUri);

			if (requestUri != null &&
				_webViewHandler != null &&
				_webViewHandler.WebviewManager != null &&
				_webViewHandler.WebviewManager.TryGetResponseContentInternal(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
			{
				var contentType = headers["Content-Type"];

				// By default local caching is disabled so that user scripts are always re-executed. Applications can
				// opt specific resources into caching via BlazorWebView.StaticContentCacheControlProvider.
				// The original (unstripped) URI is passed so the provider can act on query strings (e.g. img.png?v=2).
				// See https://github.com/dotnet/maui/issues/8279
				var cacheControlOverride = StaticContentCacheControl.ResolveOverride(_webViewHandler?.VirtualView, originalRequestUri, contentType, logger);
				if (cacheControlOverride is not null)
				{
					headers["Cache-Control"] = cacheControlOverride;
				}

				logger?.ResponseContentBeingSent(requestUri, statusCode);

				if (statusCode == 200 &&
					headers.TryGetValue("Cache-Control", out var cacheControl) &&
					StaticContentResponseCachePolicy.TryGetCacheLifetime(cacheControl, out var cacheLifetime))
				{
					cacheRequestBehavior ??= StaticContentResponseCachePolicy.GetRequestBehavior(request.Method, request.RequestHeaders);
					if (cacheRequestBehavior != StaticContentCacheRequestBehavior.Disabled)
					{
						if (StaticContentResponseBuffer.TryBuffer(content, originalRequestUri, logger, out var cachedContent, out var responseContent))
						{
							var responseToCache = new StaticContentResponse(
								originalRequestUri,
								contentType,
								statusCode,
								statusMessage,
								headers,
								cachedContent,
								StaticContentResponseCachePolicy.GetExpiration(cacheLifetime));

							_staticContentResponseCache.Set(responseToCache);
						}

						return new WebResourceResponse(contentType, "UTF-8", statusCode, statusMessage, headers, responseContent);
					}
				}

				return new WebResourceResponse(contentType, "UTF-8", statusCode, statusMessage, headers, content);
			}
			else
			{
				logger?.ResponseContentNotFound(requestUri ?? string.Empty);
			}

			return null;
		}

		private static WebResourceResponse CreateWebResourceResponse(StaticContentResponse response)
			=> new(
				response.ContentType,
				"UTF-8",
				response.StatusCode,
				response.StatusMessage,
				new Dictionary<string, string>(response.Headers, StringComparer.OrdinalIgnoreCase),
				new MemoryStream(response.Content, writable: false));

		public override void OnPageFinished(AWebView? view, string? url)
		{
			base.OnPageFinished(view, url);

			if (view != null && url != null && AppOriginUri.IsBaseOfPage(url))
			{
				// Startup scripts must run in OnPageFinished. If scripts are run earlier they will have no lasting
				// effect because once the page content loads all the document state gets reset.
				RunBlazorStartupScripts(view);
			}

			_webViewHandler?.UpdateBackNavigationState();
		}

		public override void DoUpdateVisitedHistory(AWebView? view, string? url, bool isReload)
		{
			base.DoUpdateVisitedHistory(view, url, isReload);
			// Covers Blazor client-side (SPA) navigations that use pushState/replaceState.
			// DoUpdateVisitedHistory fires for pushState on all supported Android API levels (24+).
			// replaceState does not add a new history entry so CanGoBack() is unaffected by it.
			_webViewHandler?.UpdateBackNavigationState();
		}

		private void RunBlazorStartupScripts(AWebView view)
		{
			_webViewHandler?.Logger.RunningBlazorStartupScripts();

			// Confirm Blazor hasn't already initialized
			view.EvaluateJavascript(@"
				(function() { return typeof(window.__BlazorStarted); })();
			", new JavaScriptValueCallback(blazorStarted =>
			{
				if (blazorStarted?.ToString() != "\"undefined\"")
				{
					// Blazor has already started, we can just abort startup process
					return;
				}

				// Set up JS ports
				view.EvaluateJavascript(@"

		const channel = new MessageChannel();
		var nativeJsPortOne = channel.port1;
		var nativeJsPortTwo = channel.port2;
		window.addEventListener('message', function (event) {
			if (event.data != 'capturePort') {
				nativeJsPortOne.postMessage(event.data)
			}
			else if (event.data == 'capturePort') {
				if (event.ports[0] != null) {
					nativeJsPortTwo = event.ports[0]
				}
			}
		}, false);

		nativeJsPortOne.addEventListener('message', function (event) {
		}, false);

		nativeJsPortTwo.addEventListener('message', function (event) {
			// data from native code to JS
			if (window.external.__callback) {
				window.external.__callback(event.data);
			}
		}, false);
		nativeJsPortOne.start();
		nativeJsPortTwo.start();

		window.external.sendMessage = function (message) {
			// data from JS to native code
			nativeJsPortTwo.postMessage(message);
		};

		window.external.receiveMessage = function (callback) {
			window.external.__callback = callback;
		}
				", new JavaScriptValueCallback(_ =>
					{
						// Set up Server ports
						_webViewHandler?.WebviewManager?.SetUpMessageChannel();

						// Start Blazor
						view.EvaluateJavascript(@"
							Blazor.start();
							window.__BlazorStarted = true;
						", new JavaScriptValueCallback(_ =>
						{
							// Done; no more action required
							_webViewHandler?.Logger.BlazorStartupScriptsFinished();
						}));
					}));
			}));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_staticContentResponseCache.Clear();
			}

			base.Dispose(disposing);
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
