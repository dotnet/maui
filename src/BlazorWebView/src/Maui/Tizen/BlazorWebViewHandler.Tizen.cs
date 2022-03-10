using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Tizen.NUI;
using NWebView = Tizen.NUI.BaseComponents.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, NWebView>
	{
		private const string AppOrigin = "http://0.0.0.0/";
		private const string BlazorInitScript = @"
			window.__receiveMessageCallbacks = [];
			window.__dispatchMessageCallback = function(message) {
				window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
			};
			window.external = {
				sendMessage: function(message) {
					window.BlazorHandler.postMessage(message);
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

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		protected override NWebView CreatePlatformView()
		{
			return new NWebView()
			{
				MouseEventsEnabled = true,
				KeyEventsEnabled = true,
			};
		}

		protected override void ConnectHandler(NWebView platformView)
		{
			PlatformView.PageLoadFinished += OnLoadFinished;
			PlatformView.Context.RegisterHttpRequestInterceptedCallback(OnRequestInterceptCallback);
			PlatformView.AddJavaScriptMessageHandler("BlazorHandler", PostMessageFromJS);
		}

		protected override void DisconnectHandler(NWebView platformView)
		{
			PlatformView.PageLoadFinished -= OnLoadFinished;
			base.DisconnectHandler(platformView);
		}


		public void PostMessageFromJS(string message)
		{
			_webviewManager!.MessageReceivedInternal(new Uri(PlatformView.Url), message);
		}

		private void OnLoadFinished(object? sender, WebViewPageLoadEventArgs e)
		{
			//FocusManager.Instance.SetCurrentFocusView(NativeView);
			var url = PlatformView.Url;

			if (url == AppOrigin)
				PlatformView.EvaluateJavaScript(BlazorInitScript);
		}

		private void OnRequestInterceptCallback(WebHttpRequestInterceptor interceptor)
		{
			var url = interceptor.Url;
			if (url.StartsWith(AppOrigin))
			{
				var allowFallbackOnHostPage = url.EndsWith("/");
				url = QueryStringHelper.RemovePossibleQueryString(url);
				if (_webviewManager!.TryGetResponseContentInternal(url, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
				{
					var header = $"HTTP/1.0 200 OK\r\n";
					foreach (var item in headers)
					{
						header += $"{item.Key}:{item.Value}\r\n";
					}
					header += "\r\n";
					MemoryStream memstream = new MemoryStream();
					content.CopyTo(memstream);
					interceptor.SetResponse(header, memstream.ToArray());
					return;
				}
			}
			interceptor.Ignore();
		}

		public void PostMessageFromJS(JavaScriptMessage message)
		{
			if (message is null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (message.Name.Equals("BlazorHandler", StringComparison.Ordinal))
			{
				_webviewManager!.MessageReceivedInternal(new Uri(NativeWebView.Url), message.GetBodyAsString());
			}
		}

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				_webviewManager != null)
			{
				return;
			}
			if (PlatformView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without platform web view instance.");
			}

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = System.IO.Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = System.IO.Path.GetRelativePath(contentRootDir, HostPage!);

			var customFileProvider = VirtualView.CreateFileProvider(contentRootDir);

			var resContentRootDir = Path.Combine(TApplication.Current.DirectoryInfo.Resource, contentRootDir);
			var mauiAssetFileProvider = new PhysicalFileProvider(resContentRootDir);

			var fileProvider = VirtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new TizenWebViewManager(
				this,
				PlatformView,
				Services!,
				new MauiDispatcher(Services!.GetRequiredService<IDispatcher>()),
				fileProvider,
				VirtualView.JSComponents,
				contentRootDir,
				hostPageRelativePath);

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

			VirtualView.BlazorWebViewInitializing(new BlazorWebViewInitializingEventArgs());
			VirtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs
			{
				WebView = PlatformView,
			});

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

		internal IFileProvider CreateFileProvider(string contentRootDir)
		{
			return new TizenMauiAssetFileProvider(contentRootDir);
		}

	}
}
