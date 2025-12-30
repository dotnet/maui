using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
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

		internal static void MapFlowDirection(IHybridWebViewHandler handler, IHybridWebView hybridWebView)
		{
			// Explicitly do nothing here to override the base ViewHandler.MapFlowDirection behavior
			// This prevents the WebView2.FlowDirection from being set, avoiding content mirroring
		}

		private async void OnWebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs eventArgs)
		{
			var url = eventArgs.Request.Uri;

			var logger = MauiContext?.CreateLogger<HybridWebViewHandler>();

			logger?.LogDebug("Intercepting request for {Url}.", url);

			// 1. First check if the app wants to modify or override the request.
			if (WebRequestInterceptingWebView.TryInterceptResponseStream(this, sender, eventArgs, url, logger))
			{
				return;
			}

			// 2. If this is an app request, then assume the request is for a local resource.
			if (new Uri(url) is Uri uri && AppOriginUri.IsBaseOf(uri))
			{
				logger?.LogDebug("Request for {Url} will be handled by .NET MAUI.", url);

				// 2.a. Get a deferral object so that WebView2 knows there's some async stuff going on. We call Complete() at the end of this method.
				using var deferral = eventArgs.GetDeferral();

				// 2.b. Check if the request is for a local resource
				var (stream, contentType, statusCode, reason) = await GetResponseStreamAsync(url, eventArgs.Request, logger);

				// 2.c. Create the response header
				var headers = "";
				if (stream?.Size is { } contentLength && contentLength > 0)
				{
					headers += $"Content-Length: {contentLength}{Environment.NewLine}";
				}
				if (contentType is not null)
				{
					headers += $"Content-Type: {contentType}{Environment.NewLine}";
				}

				// 2.d. If something was found, return the content
				eventArgs.Response = sender.Environment!.CreateWebResourceResponse(
					Content: stream,
					StatusCode: statusCode,
					ReasonPhrase: reason,
					Headers: headers);

				// 2.e. Notify WebView2 that the deferred (async) operation is complete and we set a response.
				deferral.Complete();
				return;
			}

			// 3. If the request is not handled by the app nor is it a local source, then we let the WebView2
			//    handle the request as it would normally do. This means that it will try to load the resource
			//    from the internet or from the local cache.

			logger?.LogDebug("Request for {Url} was not handled.", url);
		}

		private async Task<(IRandomAccessStream? Stream, string? ContentType, int StatusCode, string Reason)> GetResponseStreamAsync(string url, CoreWebView2WebResourceRequest request, ILogger? logger)
		{
			var requestUri = WebUtils.RemovePossibleQueryString(url);

			if (new Uri(requestUri) is Uri uri && AppOriginUri.IsBaseOf(uri))
			{
				var relativePath = AppOriginUri.MakeRelativeUri(uri).ToString();

				// 1.a. Try the special "_framework/hybridwebview.js" path
				if (relativePath == HybridWebViewDotJsPath)
				{
					logger?.LogDebug("Request for {Url} will return the hybrid web view script.", url);
					var jsStream = GetEmbeddedStream(HybridWebViewDotJsPath);
					if (jsStream is not null)
					{
						var ras = await CopyContentToRandomAccessStreamAsync(jsStream);
						return (Stream: ras, ContentType: "application/javascript", StatusCode: 200, Reason: "OK");
					}
				}

				// 1.b. Try special InvokeDotNet path
				if (relativePath == InvokeDotNetPath)
				{
					logger?.LogDebug("Request for {Url} will be handled by the .NET method invoker.", url);

					// Only accept requests that have the expected headers
					if (!HasExpectedHeaders(request.Headers))
					{
						logger?.LogError("InvokeDotNet endpoint missing or invalid request header");
						return (Stream: null, ContentType: null, StatusCode: 400, Reason: "Bad Request");
					}

					// Only accept POST requests
					if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
					{
						logger?.LogError("InvokeDotNet endpoint only accepts POST requests. Received: {Method}", request.Method);
						return (Stream: null, ContentType: null, StatusCode: 405, Reason: "Method Not Allowed");
					}

					// Read the request body
					Stream requestBody;
					if (request.Content is { } bodyStream && bodyStream.Size > 0)
					{
						requestBody = bodyStream.AsStreamForRead();
					}
					else
					{
						logger?.LogError("InvokeDotNet request body is empty");
						return (Stream: null, ContentType: null, StatusCode: 400, Reason: "Bad Request");
					}

					// Invoke the method
					var contentBytes = await InvokeDotNetAsync(streamBody: requestBody);
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
						contentType = "text/plain";
						logger?.LogWarning("Could not determine content type for '{relativePath}'", relativePath);
					}
				}

				var assetPath = Path.Combine(VirtualView.HybridRoot!, relativePath!);
				using var contentStream = await GetAssetStreamAsync(assetPath);

				if (contentStream is not null)
				{
					// 3.a. If something was found, return the content
					logger?.LogDebug("Request for {Url} will return an app package file.", url);

					var ras = await CopyContentToRandomAccessStreamAsync(contentStream);
					return (Stream: ras, ContentType: contentType, StatusCode: 200, Reason: "OK");
				}
			}

			// 3.b. Otherwise, return a 404
			logger?.LogDebug("Request for {Url} could not be fulfilled.", url);
			return (Stream: null, ContentType: null, StatusCode: 404, Reason: "Not Found");
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
			private IHybridWebView? VirtualView => Handler?.VirtualView;

			public void Connect(HybridWebViewHandler handler, WebView2 platformView)
			{
				_handler = new(handler);

				(platformView as MauiHybridWebView)!.WebViewReadyTask = TryInitializeWebView2(platformView);
			}

			private async Task<bool> TryInitializeWebView2(WebView2 webView)
			{
				// Invoke the WebViewInitializing event to allow custom configuration of the web view
				var initializingArgs = new WebViewInitializationStartedEventArgs();
				VirtualView?.WebViewInitializationStarted(initializingArgs);

				var env = await CoreWebView2Environment.CreateWithOptionsAsync(
					browserExecutableFolder: initializingArgs.BrowserExecutableFolder,
					userDataFolder: initializingArgs.UserDataFolder,
					options: initializingArgs.EnvironmentOptions);

				var options = env.CreateCoreWebView2ControllerOptions();
				options.ScriptLocale = initializingArgs.ScriptLocale;
				options.IsInPrivateModeEnabled = initializingArgs.IsInPrivateModeEnabled;
				options.ProfileName = initializingArgs.ProfileName;

				await webView.EnsureCoreWebView2Async(env, options);

				webView.CoreWebView2.Settings.AreDevToolsEnabled = Handler?.DeveloperTools.Enabled ?? false;
				webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

				webView.CoreWebView2.AddWebResourceRequestedFilter($"*", CoreWebView2WebResourceContext.All);

				// Invoke the WebViewInitialized event to signal that the web view has been initialized
				var initializedArgs = new WebViewInitializationCompletedEventArgs(webView.CoreWebView2, webView.CoreWebView2.Settings);
				VirtualView?.WebViewInitializationCompleted(initializedArgs);

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
