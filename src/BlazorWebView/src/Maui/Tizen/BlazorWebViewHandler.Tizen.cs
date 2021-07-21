using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Handlers;
using TChromium = Tizen.WebView.Chromium;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WebViewContainer>
	{
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

		private TizenWebViewManager? _webviewManager;
		private WebViewExtension.InterceptRequestCallback? _interceptRequestCallback;

		private TWebView NativeWebView => NativeView.WebView;

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		protected override WebViewContainer CreateNativeView()
		{
			TChromium.Initialize();
			Context!.CurrentApplication!.Terminated += (s, e) => TChromium.Shutdown();
			return new WebViewContainer(Context!.NativeParent);
		}

		protected override void ConnectHandler(WebViewContainer nativeView)
		{
			_interceptRequestCallback = OnRequestInterceptCallback;
			NativeWebView.LoadStarted += OnLoadStarted;
			NativeWebView.LoadFinished += OnLoadFinished;
			//NativeWebView.AddJavaScriptMessageHandler("BlazorHandler", PostMessageFromJS);
			NativeWebView.SetInterceptRequestCallback(_interceptRequestCallback);
			NativeWebView.GetSettings().JavaScriptEnabled = true;

		}

		protected override void DisconnectHandler(WebViewContainer nativeView)
		{
			base.DisconnectHandler(nativeView);
		}

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				_webviewManager != null)
			{
				return;
			}
			if (NativeView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			var assetConfig = Services!.GetRequiredService<BlazorAssetsAssemblyConfiguration>()!;

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			var fileProvider = new ManifestEmbeddedFileProvider(assetConfig.AssetsAssembly, root: contentRootDir);

			_webviewManager = new TizenWebViewManager(this, NativeWebView, Services!, MauiDispatcher.Instance, fileProvider, hostPageRelativePath);
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

		private void OnRequestInterceptCallback(IntPtr context, IntPtr request, IntPtr userdata)
		{
			if (request == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var url = NativeWebView.GetInterceptRequestUrl(request);
			var urlScheme = url.Substring(0, url.IndexOf(':'));

			if (urlScheme == "app")
			{
				var allowFallbackOnHostPage = url.EndsWith("/");
				if (_webviewManager!.TryGetResponseContentInternal(url, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
				{
					var contentType = headers["Content-Type"];
					var header = $"HTTP/1.0 200 OK\r\nContent-Type:{contentType}; charset=utf-8\r\nCache-Control:no-cache, max-age=0, must-revalidate, no-store\r\n\r\n";
					var body = new StreamReader(content).ReadToEnd();
					NativeWebView.SetInterceptRequestResponse(request, header, body, (uint)body.Length);
					return;
				}
			}

			NativeWebView.IgnoreInterceptRequest(request);
		}

		private void OnLoadStarted(object? sender, EventArgs e)
		{
		}

		private void OnLoadFinished(object? sender, EventArgs e)
		{
			NativeWebView.SetFocus(true);

			var url = NativeWebView.Url;
			if (url == AppOrigin)
				NativeWebView.Eval(BlazorInitScript);
		}
	}
}
