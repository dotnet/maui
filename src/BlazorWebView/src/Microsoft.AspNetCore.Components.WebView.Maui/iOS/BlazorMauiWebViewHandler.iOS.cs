using System;
using System.Collections.ObjectModel;
using System.IO;
using Foundation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using System.Linq;
using UIKit;
using WebKit;
using System.Globalization;
using System.Collections.Generic;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WKWebView>
	{
		protected override WKWebView CreateNativeView()
		{
			var config = new WKWebViewConfiguration();

			config.Preferences.SetValueForKey(NSObject.FromObject(true), new NSString("developerExtrasEnabled"));

			var frameworkScriptSource = @"

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

			config.UserContentController.AddScriptMessageHandler(new WebViewScriptMessageHandler(MessageReceived), "webwindowinterop");
			config.UserContentController.AddUserScript(new WKUserScript(
				new NSString(frameworkScriptSource), WKUserScriptInjectionTime.AtDocumentEnd, true));

			// iOS WKWebView doesn't allow handling 'http'/'https' schemes, so we use the fake 'app' scheme
			config.SetUrlSchemeHandler(new SchemeHandler(this), urlScheme: "app");


			return new WKWebView(RectangleF.Empty, config)
			{
				BackgroundColor = UIColor.Clear,
				AutosizesSubviews = true
			};
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
				System.Console.WriteLine($"1111111111-StartUrlSchemeTask for URL: {urlSchemeTask.Request.Url.AbsoluteString}");

				var responseBytes = GetResponseBytes(urlSchemeTask.Request.Url.AbsoluteString, out var contentType, statusCode: out var statusCode);
				if (statusCode == 200)
				{
					System.Console.WriteLine($"1111111111-StartUrlSchemeTask - FOUND! {urlSchemeTask.Request.Url.AbsoluteString}");
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

					var cc2 = cc.Substring(0, Math.Min(50, cc.Length));
					System.Console.WriteLine($"1111111111-StartUrlSchemeTask - FOUND '{url}'! Content: MIME='{contentType}', content={cc2}");

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


		private const string AppOrigin = "app://0.0.0.0/";

		void MessageReceived(Uri uri, string message)
		{
			_webviewManager?.MessageReceivedInternal(uri, message);
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


		private IOSWebViewManager? _webviewManager;

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

		public string? HostPage { get; private set; }
		public ObservableCollection<RootComponent>? RootComponents { get; private set; }
		public new IServiceProvider? Services { get; private set; }

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

			System.Console.WriteLine($"1111111111-StartWebViewCoreIfPossible - HostPage={HostPage}, contentRootDir={contentRootDir}, hostPageRelativePath={hostPageRelativePath}");


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
	}
}
