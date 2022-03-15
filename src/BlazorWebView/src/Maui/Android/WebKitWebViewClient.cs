using System;
using System.IO;
using System.Web;
using Android.Content;
using Android.Runtime;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;
using AUri = Android.Net.Uri;
using Java.Net;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class WebKitWebViewClient : WebViewClient
	{
		// Using an IP address means that WebView doesn't wait for any DNS resolution,
		// making it substantially faster. Note that this isn't real HTTP traffic, since
		// we intercept all the requests within this origin.
		private static readonly string AppOrigin = $"https://{BlazorWebView.AppHostAddress}/";

		private static readonly Uri AppOriginUri = new(AppOrigin);

		private readonly BlazorWebViewHandler? _webViewHandler;

		public WebKitWebViewClient(BlazorWebViewHandler webViewHandler!!)
		{
			_webViewHandler = webViewHandler;
		}

		protected WebKitWebViewClient(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			// This constructor is called whenever the .NET proxy was disposed, and it was recreated by Java. It also
			// happens when overridden methods are called between execution of this constructor and the one above.
			// because of these facts, we have to check all methods below for null field references and properties.
		}

		public override bool ShouldOverrideUrlLoading(AWebView? view, IWebResourceRequest? request)
		{
			// Handle redirects to the app custom scheme by reloading the URL in the view.
			// Handle navigation to external URLs using the system browser, unless overriden.
			if (request?.Url is { } url)
			{
				if (url.Scheme is "http" or "https")
				{
					if (TryOverrideHttpUrlLoading(view, request, url.ToString()))
					{
						return true;
					}
				}
				else
				{
					TryStartActivityFromUri(url.ToString(), uriHasIntentScheme: url.Scheme == "intent");
					return true;
				}
			}

			return base.ShouldOverrideUrlLoading(view, request);
		}

		private bool TryOverrideHttpUrlLoading(AWebView? view, IWebResourceRequest? request, string? requestUri)
		{
			if (Uri.TryCreate(requestUri, UriKind.RelativeOrAbsolute, out var uri))
			{
				if (uri.Host == BlazorWebView.AppHostAddress &&
					view is not null &&
					request is not null &&
					request.IsRedirect &&
					request.IsForMainFrame)
				{
					view.LoadUrl(uri.ToString());
					return true;
				}
				else if (uri.Host != BlazorWebView.AppHostAddress && _webViewHandler != null)
				{
					var callbackArgs = new ExternalLinkNavigationEventArgs(uri);
					_webViewHandler.ExternalNavigationStarting?.Invoke(callbackArgs);

					if (callbackArgs.ExternalLinkNavigationPolicy == ExternalLinkNavigationPolicy.OpenInExternalBrowser)
					{
						var intent = new Intent(Intent.ActionView, AUri.Parse(requestUri));
						_webViewHandler.Context.StartActivity(intent);
					}

					if (callbackArgs.ExternalLinkNavigationPolicy != ExternalLinkNavigationPolicy.InsecureOpenInWebView)
					{
						return true;
					}
				}
			}

			return false;
		}

		private void TryStartActivityFromUri(string? uriString, bool uriHasIntentScheme)
		{
			if (_webViewHandler is null || uriString is null)
			{
				return;
			}

			try
			{
				var intent = Intent.ParseUri(uriString, IntentUriType.Scheme);

				if (intent is not null)
				{
					_webViewHandler.Context.StartActivity(intent);
				}
			}
			catch (URISyntaxException)
			{
				// No-op. The URI was malformed.
				return;
			}
			catch (ActivityNotFoundException)
			{
				if (!uriHasIntentScheme)
				{
					// No-op. This behavior is consistent with other platforms when a URI with an unknown scheme is clicked.
					return;
				}

				// Chrome for Android allows specifying a fallback URL in case an intent cannot be resolved.
				// Since the Android WebView cannot handle "intent"-schemed URIs, we add our own support for the fallback
				// URL here.
				// See: https://developer.chrome.com/docs/multidevice/android/intents/

				const string FallbackUrlQueryParameterPrefix = "S.browser_fallback_url=";

				var fallbackUrlQueryParameterPrefixLocation = uriString.LastIndexOf(FallbackUrlQueryParameterPrefix);

				if (fallbackUrlQueryParameterPrefixLocation != -1)
				{
					var fallbackUrlQueryParameterValueStart = fallbackUrlQueryParameterPrefixLocation + FallbackUrlQueryParameterPrefix.Length;
					var fallbackUrlQueryParameterValueEnd = uriString.IndexOf(';', fallbackUrlQueryParameterValueStart);

					if (fallbackUrlQueryParameterValueEnd != -1)
					{
						var encodedFallbackUrlString = uriString[fallbackUrlQueryParameterValueStart..fallbackUrlQueryParameterValueEnd];
						var fallbackUrlString = HttpUtility.UrlDecode(encodedFallbackUrlString);
						TryStartActivityFromUri(fallbackUrlString, uriHasIntentScheme: false);
					}
				}
			}
		}

		public override WebResourceResponse? ShouldInterceptRequest(AWebView? view, IWebResourceRequest? request)
		{
			if (request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestUri = request?.Url?.ToString();
			var allowFallbackOnHostPage = IsAppOriginPageUri(requestUri);
			requestUri = QueryStringHelper.RemovePossibleQueryString(requestUri);

			if (requestUri != null &&
				_webViewHandler != null &&
				_webViewHandler.WebviewManager != null &&
				_webViewHandler.WebviewManager.TryGetResponseContentInternal(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
			{
				var contentType = headers["Content-Type"];

				return new WebResourceResponse(contentType, "UTF-8", statusCode, statusMessage, headers, content);
			}

			return base.ShouldInterceptRequest(view, request);
		}

		public override void OnPageFinished(AWebView? view, string? url)
		{
			base.OnPageFinished(view, url);

			// TODO: How do we know this runs only once?
			if (view != null && IsAppOriginPageUri(url))
			{
				// Startup scripts must run in OnPageFinished. If scripts are run earlier they will have no lasting
				// effect because once the page content loads all the document state gets reset.
				RunBlazorStartupScripts(view);
			}
		}

		private void RunBlazorStartupScripts(AWebView view)
		{
			// TODO: we need to protect against double initialization because the
			// OnPageFinished event refires after the app is brought back from the 
			// foreground and the webview is brought back into view, without it actually
			// getting reloaded.


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

		", new JavaScriptValueCallback(() =>
			{
				// Set up Server ports
				_webViewHandler?.WebviewManager?.SetUpMessageChannel();

				// Start Blazor
				view.EvaluateJavascript(@"
					Blazor.start();
				", new JavaScriptValueCallback(() =>
				{
					// Done; no more action required
				}));
			}));
		}

		private static bool IsAppOriginPageUri(string? requestUriString)
		{
			if (string.IsNullOrEmpty(requestUriString))
			{
				return false;
			}

			var requestUri = new Uri(requestUriString);
			if (!AppOriginUri.IsBaseOf(requestUri))
			{
				return false;
			}

			// If the path does not end in a file extension, it's most likely referring to a page.
			return !Path.HasExtension(requestUriString);
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
			private readonly Action _callback;

			public JavaScriptValueCallback(Action callback!!)
			{
				_callback = callback;
			}

			public void OnReceiveValue(Java.Lang.Object? value)
			{
				_callback();
			}
		}
	}
}
