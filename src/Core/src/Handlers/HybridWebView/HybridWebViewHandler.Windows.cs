using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Streams;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, WebView2>
	{
		private readonly HybridWebView2Proxy _proxy = new();

		protected override WebView2 CreatePlatformView()
		{
			return new MauiHybridWebView(this);
		}

		protected override void ConnectHandler(WebView2 platformView)
		{
			_proxy.Connect(this, platformView);

			base.ConnectHandler(platformView);

			if (platformView.IsLoaded)
			{
				OnLoaded();
			}
			else
			{
				platformView.Loaded += OnWebViewLoaded;
			}

			PlatformView.DispatcherQueue.TryEnqueue(async () =>
			{
				var isWebViewInitialized = await (PlatformView as MauiHybridWebView)!.WebViewReadyTask!;

				if (isWebViewInitialized)
				{
					PlatformView.Source = new Uri(new Uri(AppOriginUri, "/").ToString());
				}
			});

		}

		void OnWebViewLoaded(object sender, RoutedEventArgs e)
		{
			OnLoaded();
		}

		void OnLoaded()
		{
			var window = MauiContext!.GetPlatformWindow();
			_proxy.Connect(window);
		}

		void Disconnect(WebView2 platformView)
		{
			platformView.Loaded -= OnWebViewLoaded;
			_proxy.Disconnect(platformView);
			if (platformView.CoreWebView2 is not null)
			{
				platformView.Close();
			}
		}

		protected override void DisconnectHandler(WebView2 platformView)
		{
			Disconnect(platformView);
			base.DisconnectHandler(platformView);
		}

		internal static void EvaluateJavaScript(IHybridWebViewHandler handler, IHybridWebView hybridWebView, EvaluateJavaScriptAsyncRequest request)
		{
			if (handler.PlatformView is not MauiHybridWebView hybridPlatformWebView)
			{
				return;
			}

			hybridPlatformWebView.RunAfterInitialize(() => hybridPlatformWebView.EvaluateJavaScript(request));
		}

		public static void MapSendRawMessage(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
			if (arg is not HybridWebViewRawMessage hybridWebViewRawMessage || handler.PlatformView is not MauiHybridWebView hybridPlatformWebView)
			{
				return;
			}

			hybridPlatformWebView.RunAfterInitialize(() => hybridPlatformWebView.SendRawMessage(hybridWebViewRawMessage.Message ?? ""));
		}

		private void OnWebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
		{
			MessageReceived(args.TryGetWebMessageAsString());
		}

		private async void OnWebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs eventArgs)
		{
			// Get a deferral object so that WebView2 knows there's some async stuff going on. We call Complete() at the end of this method.
			using var deferral = eventArgs.GetDeferral();

			var requestUri = HybridWebViewQueryStringHelper.RemovePossibleQueryString(eventArgs.Request.Uri);

			if (new Uri(requestUri) is Uri uri && AppOriginUri.IsBaseOf(uri))
			{
				var relativePath = AppOriginUri.MakeRelativeUri(uri).ToString().Replace('/', '\\');

				string contentType;
				if (string.IsNullOrEmpty(relativePath))
				{
					relativePath = VirtualView.DefaultFile;
					contentType = "text/html";
				}
				else
				{
					if (!ContentTypeProvider.TryGetContentType(relativePath, out contentType!))
					{
						// TODO: Log this
						contentType = "text/plain";
					}
				}

				var assetPath = Path.Combine(VirtualView.HybridRoot!, relativePath!);
				var contentStream = await GetAssetStreamAsync(assetPath);

				if (contentStream is null)
				{
					var notFoundContent = "Resource not found (404)";
					eventArgs.Response = sender.Environment!.CreateWebResourceResponse(
						Content: null,
						StatusCode: 404,
						ReasonPhrase: "Not Found",
						Headers: GetHeaderString("text/plain", notFoundContent.Length)
					);
				}
				else
				{
					eventArgs.Response = sender.Environment!.CreateWebResourceResponse(
						Content: await CopyContentToRandomAccessStreamAsync(contentStream),
						StatusCode: 200,
						ReasonPhrase: "OK",
						Headers: GetHeaderString(contentType, (int)contentStream.Length)
					);
				}

				contentStream?.Dispose();
			}

			// Notify WebView2 that the deferred (async) operation is complete and we set a response.
			deferral.Complete();

			async Task<IRandomAccessStream> CopyContentToRandomAccessStreamAsync(Stream content)
			{
				using var memStream = new MemoryStream();
				await content.CopyToAsync(memStream);
				var randomAccessStream = new InMemoryRandomAccessStream();
				await randomAccessStream.WriteAsync(memStream.GetWindowsRuntimeBuffer());
				return randomAccessStream;
			}
		}

		private protected static string GetHeaderString(string contentType, int contentLength) =>
$@"Content-Type: {contentType}
Content-Length: {contentLength}";

		private sealed class HybridWebView2Proxy
		{
			private WeakReference<Window>? _window;
			private WeakReference<HybridWebViewHandler>? _handler;

			private Window? Window => _window is not null && _window.TryGetTarget(out var w) ? w : null;
			private HybridWebViewHandler? Handler => _handler is not null && _handler.TryGetTarget(out var h) ? h : null;

			public void Connect(HybridWebViewHandler handler, WebView2 platformView)
			{
				_handler = new(handler);

				(platformView as MauiHybridWebView)!.WebViewReadyTask = TryInitializeWebView2(platformView);
			}

			private async Task<bool> TryInitializeWebView2(WebView2 webView)
			{
				await webView.EnsureCoreWebView2Async();

				webView.CoreWebView2.Settings.AreDevToolsEnabled = Handler?.DeveloperTools.Enabled ?? false;
				webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
				webView.CoreWebView2.AddWebResourceRequestedFilter($"{AppOrigin}*", CoreWebView2WebResourceContext.All);

				webView.WebMessageReceived += OnWebMessageReceived;
				webView.CoreWebView2.WebResourceRequested += OnWebResourceRequested;

				return true;
			}

			private void OnWebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs args)
			{
				Handler?.OnWebResourceRequested(sender, args);
			}

			public void Connect(Window window)
			{
				_window = new(window);
				window.Closed += OnWindowClosed;
			}

			public void Disconnect(WebView2 platformView)
			{
				platformView.WebMessageReceived -= OnWebMessageReceived;
				platformView.CoreWebView2.WebResourceRequested -= OnWebResourceRequested;

				if (platformView.CoreWebView2 is CoreWebView2 webView2)
				{
					webView2.Stop();
				}

				if (Window is Window window)
				{
					window.Closed -= OnWindowClosed;
				}

				_handler = null;
				_window = null;
			}

			void OnWebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
			{
				if (Handler is HybridWebViewHandler handler)
				{
					handler.OnWebMessageReceived(sender, args);
				}
			}

			void OnWindowClosed(object sender, WindowEventArgs args)
			{
				if (Handler is HybridWebViewHandler handler)
				{
					handler.Disconnect(handler.PlatformView);
				}
			}
		}
	}
}
