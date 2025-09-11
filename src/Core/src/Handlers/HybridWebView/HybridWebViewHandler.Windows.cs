using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Streams;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, WebView2>
	{
		private readonly HybridWebView2Proxy _proxy = new();
		private readonly Lazy<IBuffer> _404MessageBuffer = new(() => Encoding.UTF8.GetBytes("Resource not found (404)").AsBuffer());

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

		internal static void MapFlowDirection(IHybridWebViewHandler handler, IHybridWebView hybridWebView)
		{
			// Explicitly do nothing here to override the base ViewHandler.MapFlowDirection behavior
			// This prevents the WebView2.FlowDirection from being set, avoiding content mirroring
		}

		private async void OnWebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs eventArgs)
		{
			// Get a deferral object so that WebView2 knows there's some async stuff going on. We call Complete() at the end of this method.
			using var deferral = eventArgs.GetDeferral();

			var (stream, contentType, statusCode, reason) = await GetResponseStreamAsync(eventArgs.Request.Uri);
			var contentLength = stream?.Size ?? 0;
			var headers =
				$"""
				Content-Type: {contentType}
				Content-Length: {contentLength}
				""";

			eventArgs.Response = sender.Environment!.CreateWebResourceResponse(
				Content: stream,
				StatusCode: statusCode,
				ReasonPhrase: reason,
				Headers: headers);

			// Notify WebView2 that the deferred (async) operation is complete and we set a response.
			deferral.Complete();
		}

		private async Task<(IRandomAccessStream Stream, string ContentType, int StatusCode, string Reason)> GetResponseStreamAsync(string url)
		{
			var requestUri = HybridWebViewQueryStringHelper.RemovePossibleQueryString(url);

			if (new Uri(requestUri) is Uri uri && AppOriginUri.IsBaseOf(uri))
			{
				var relativePath = AppOriginUri.MakeRelativeUri(uri).ToString().Replace('/', '\\');

				// 1. Try special InvokeDotNet path
				if (relativePath == InvokeDotNetPath)
				{
					var fullUri = new Uri(url);
					var invokeQueryString = HttpUtility.ParseQueryString(fullUri.Query);
					var contentBytes = await InvokeDotNetAsync(invokeQueryString);
					if (contentBytes is not null)
					{
						var ras = await CopyContentToRandomAccessStreamAsync(contentBytes.AsBuffer());
						return (Stream: ras, ContentType: "application/json", StatusCode: 200, Reason: "OK");
					}
				}

				string contentType;

				// 2. If nothing found yet, try to get static content from the asset path
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
				using var contentStream = await GetAssetStreamAsync(assetPath);

				if (contentStream is not null)
				{
					// 3.a. If something was found, return the content
					var ras = await CopyContentToRandomAccessStreamAsync(contentStream);
					return (Stream: ras, ContentType: contentType, StatusCode: 200, Reason: "OK");
				}
			}

			// 3.b. Otherwise, return a 404
			var ras404 = await CopyContentToRandomAccessStreamAsync(_404MessageBuffer.Value);
			return (Stream: ras404, ContentType: "text/plain", StatusCode: 404, Reason: "Not Found");
		}

		static async Task<IRandomAccessStream> CopyContentToRandomAccessStreamAsync(Stream content)
		{
			var ras = new InMemoryRandomAccessStream();
			var stream = ras.AsStreamForWrite(); // do not dispose as this stream IS the IMRAS
			await content.CopyToAsync(stream);
			await stream.FlushAsync();
			ras.Seek(0);
			return ras;
		}

		static async Task<IRandomAccessStream> CopyContentToRandomAccessStreamAsync(IBuffer content)
		{
			var ras = new InMemoryRandomAccessStream();
			await ras.WriteAsync(content);
			await ras.FlushAsync();
			ras.Seek(0);
			return ras;
		}

		[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
		[RequiresDynamicCode(DynamicFeatures)]
#endif
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
