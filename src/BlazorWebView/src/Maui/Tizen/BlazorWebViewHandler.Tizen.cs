using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Tizen.WebView;
using TChromium = Tizen.WebView.Chromium;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// The Tizen <see cref="ViewHandler"/> for <see cref="BlazorWebView"/>.
	/// </summary>
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WebViewContainer>
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
		private WebViewExtensions.InterceptRequestCallback? _interceptRequestCallback;

		private TWebView NativeWebView => PlatformView.WebView;

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		/// <inheritdoc />
		protected override WebViewContainer CreatePlatformView()
		{
			TChromium.Initialize();
			MauiApplication.Current.Terminated += (s, e) => TChromium.Shutdown();

			return new WebViewContainer(NativeParent!);
		}

		/// <inheritdoc />
		protected override void ConnectHandler(WebViewContainer platformView)
		{
			_interceptRequestCallback = OnRequestInterceptCallback;
			NativeWebView.LoadFinished += OnLoadFinished;
			NativeWebView.AddJavaScriptMessageHandler("BlazorHandler", PostMessageFromJS);
			NativeWebView.SetInterceptRequestCallback(_interceptRequestCallback);
			NativeWebView.GetSettings().JavaScriptEnabled = true;
		}

		/// <inheritdoc />
		protected override void DisconnectHandler(WebViewContainer platformView)
		{
			NativeWebView.LoadFinished -= OnLoadFinished;
			base.DisconnectHandler(platformView);
		}

		private void PostMessageFromJS(JavaScriptMessage message)
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
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			var fileProvider = VirtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new TizenWebViewManager(
				this,
				NativeWebView,
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
				WebView = NativeWebView,
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

		private void OnRequestInterceptCallback(IntPtr context, IntPtr request, IntPtr userdata)
		{
			if (request == IntPtr.Zero)
			{
				return;
			}

			var url = NativeWebView.GetInterceptRequestUrl(request);

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

					using (MemoryStream memstream = new MemoryStream())
					{
						content.CopyTo(memstream);
						var body = memstream.ToArray();
						NativeWebView.SetInterceptRequestResponse(request, header, body, (uint)body.Length);
					}
					return;
				}
			}

			NativeWebView.IgnoreInterceptRequest(request);
		}

		private void OnLoadFinished(object? sender, EventArgs e)
		{
			NativeWebView.SetFocus(true);
			var url = NativeWebView.Url;

			if (url == AppOrigin)
				NativeWebView.Eval(BlazorInitScript);
		}

		internal IFileProvider CreateFileProvider(string contentRootDir)
		{
			return new TizenMauiAssetFileProvider(contentRootDir);
		}

	}
}
