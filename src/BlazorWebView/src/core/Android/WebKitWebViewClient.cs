using System;
using Android.Runtime;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class WebKitWebViewClient : WebViewClient
	{
		private const string AppOrigin = "https://0.0.0.0/";

		private readonly BlazorWebViewHandler? _webViewHandler;

		public WebKitWebViewClient(BlazorWebViewHandler webViewHandler)
		{
			_webViewHandler = webViewHandler ?? throw new ArgumentNullException(nameof(webViewHandler));
		}

		protected WebKitWebViewClient(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			// This constructor is called whenever the .NET proxy was disposed, and it was recreated by Java. It also
			// happens when overridden methods are called between execution of this constructor and the one above.
			// because of these facts, we have to check
			// all methods below for null field references and properties.
		}

		public override bool ShouldOverrideUrlLoading(AWebView? view, IWebResourceRequest? request)
		{
			// handle redirects to the app custom scheme by reloading the url in the view.
			// otherwise they will be blocked by Android.
			var requestUri = request?.Url?.ToString();
			if (requestUri != null && view != null &&
				request != null && request.IsRedirect && request.IsForMainFrame)
			{
				var uri = new Uri(requestUri);
				if (uri.Host == "0.0.0.0")
				{
					view.LoadUrl(uri.ToString());
					return true;
				}
			}
			return base.ShouldOverrideUrlLoading(view, request);
		}

		public override WebResourceResponse? ShouldInterceptRequest(AWebView? view, IWebResourceRequest? request)
		{
			if (request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var allowFallbackOnHostPage = false;

			var requestUri = request?.Url?.ToString();
			var appBaseUri = new Uri(AppOrigin);
			var fileUri = requestUri != null ? new Uri(requestUri) : null;

			if (fileUri != null && appBaseUri.IsBaseOf(fileUri))
			{
				var relativePath = appBaseUri.MakeRelativeUri(fileUri).ToString();
				if (string.IsNullOrEmpty(relativePath))
				{
					// For app root, use host page (something like wwwroot/index.html)
					allowFallbackOnHostPage = true;
				}
			}

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
			if (view != null && url == AppOrigin)
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

			public JavaScriptValueCallback(Action callback)
			{
				_callback = callback ?? throw new ArgumentNullException(nameof(callback));
			}

			public void OnReceiveValue(Java.Lang.Object? value)
			{
				_callback();
			}
		}
	}
}
