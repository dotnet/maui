using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Foundation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using WebKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WKWebView>
	{
		private IOSWebViewManager? _webviewManager;

		private const string AppOrigin = "app://0.0.0.0/";
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

		protected override WKWebView CreateNativeView()
		{
			var config = new WKWebViewConfiguration();

			config.Preferences.SetValueForKey(NSObject.FromObject(true), new NSString("developerExtrasEnabled"));

			config.UserContentController.AddScriptMessageHandler(new WebViewScriptMessageHandler(MessageReceived), "webwindowinterop");
			config.UserContentController.AddUserScript(new WKUserScript(
				new NSString(BlazorInitScript), WKUserScriptInjectionTime.AtDocumentEnd, true));

			// iOS WKWebView doesn't allow handling 'http'/'https' schemes, so we use the fake 'app' scheme
			config.SetUrlSchemeHandler(new SchemeHandler(this), urlScheme: "app");

			return new WKWebView(RectangleF.Empty, config)
			{
				BackgroundColor = UIColor.Clear,
				AutosizesSubviews = true
			};
		}

		private void MessageReceived(Uri uri, string message)
		{
			_webviewManager?.MessageReceivedInternal(uri, message);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			SetDesiredSize(widthConstraint, heightConstraint);

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		void SetDesiredSize(double width, double height)
		{
			if (NativeView != null)
			{
				var x = NativeView.Frame.X;
				var y = NativeView.Frame.Y;

				NativeView.Frame = new RectangleF(x, y, width, height);
			}
		}

		protected override void DisconnectHandler(WKWebView nativeView)
		{
			//nativeView.StopLoading();

			//_webViewClient?.Dispose();
			//_webChromeClient?.Dispose();
		}

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		private string? HostPage { get; set; }
		private ObservableCollection<RootComponent>? RootComponents { get; set; }
		private new IServiceProvider? Services { get; set; }

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				false)//_webviewManager != null)
			{
				return;
			}
			if (NativeView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			var resourceAssembly = RootComponents?[0]?.ComponentType?.Assembly;
			if (resourceAssembly == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without a component type assembly.");
			}

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);
			var fileProvider = new ManifestEmbeddedFileProvider(resourceAssembly, root: contentRootDir);

			_webviewManager = new IOSWebViewManager(this, NativeView, Services!, MauiDispatcher.Instance, fileProvider, hostPageRelativePath);
			if (RootComponents != null)
			{
				foreach (var rootComponent in RootComponents)
				{
					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}
			_webviewManager.Navigate("/");
		}

		public static void MapHostPage(BlazorWebViewHandler handler, IBlazorWebView webView)
		{
			handler.HostPage = webView.HostPage;
			handler.StartWebViewCoreIfPossible();
		}

		public static void MapRootComponents(BlazorWebViewHandler handler, IBlazorWebView webView)
		{
			handler.RootComponents = webView.RootComponents;
			handler.StartWebViewCoreIfPossible();
		}

		public static void MapServices(BlazorWebViewHandler handler, IBlazorWebView webView)
		{
			handler.Services = webView.Services;
			handler.StartWebViewCoreIfPossible();
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
				_messageReceivedAction(new Uri(AppOrigin), ((NSString)message.Body).ToString());
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
			public void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
			{
				var responseBytes = GetResponseBytes(urlSchemeTask.Request.Url.AbsoluteString, out var contentType, statusCode: out var statusCode);
				if (statusCode == 200)
				{
					using (var dic = new NSMutableDictionary<NSString, NSString>())
					{
						dic.Add((NSString)"Content-Length", (NSString)(responseBytes.Length.ToString(CultureInfo.InvariantCulture)));
						dic.Add((NSString)"Content-Type", (NSString)contentType);
						// Disable local caching. This will prevent user scripts from executing correctly.
						dic.Add((NSString)"Cache-Control", (NSString)"no-cache, max-age=0, must-revalidate, no-store");
						using var response = new NSHttpUrlResponse(urlSchemeTask.Request.Url, statusCode, "HTTP/1.1", dic);
						urlSchemeTask.DidReceiveResponse(response);
					}
					urlSchemeTask.DidReceiveData(NSData.FromArray(responseBytes));
					urlSchemeTask.DidFinish();
				}
			}

			private byte[] GetResponseBytes(string url, out string contentType, out int statusCode)
			{
				var allowFallbackOnHostPage = url.EndsWith("/");
				if (_webViewHandler._webviewManager!.TryGetResponseContentInternal(url, allowFallbackOnHostPage, out statusCode, out var statusMessage, out var content, out var headers))
				{
					statusCode = 200;
					using var ms = new MemoryStream();
					var uri = new Uri(url);

					content.CopyTo(ms);

					content.Position = 0;
					var sr = new StreamReader(content);
					var cc = sr.ReadToEnd();

					content.Dispose();


					var headersDict =
						new Dictionary<string, string>(
						headers
							.Split(Environment.NewLine)
							.Select(headerString =>
								new KeyValuePair<string, string>(
									headerString.Substring(0, headerString.IndexOf(':')),
									headerString.Substring(headerString.IndexOf(':') + 2))));

					contentType = headersDict["Content-Type"];
					if (cc.StartsWith("<!DOCTYPE", StringComparison.Ordinal))
					{
						contentType = "text/html";
					}
					else if (cc.Contains("box-shadow", StringComparison.Ordinal))
					{
						contentType = "text/css";
					}
					else if (url.Contains(".js", StringComparison.Ordinal))
					{
						contentType = "application/javascript";
					}

					return ms.ToArray();
				}
				else
				{
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
