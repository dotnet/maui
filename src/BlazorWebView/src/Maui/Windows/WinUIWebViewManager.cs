using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using Microsoft.Web.WebView2.Core;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// An implementation of <see cref="WebViewManager"/> that uses the Edge WebView2 browser control
	/// to render web content in WinUI applications.
	/// </summary>
	internal class WinUIWebViewManager : WebView2WebViewManager
	{
		private readonly BlazorWebViewHandler _handler;
		private readonly WebView2Control _webview;
		private readonly string _hostPageRelativePath;
		private readonly string _contentRootRelativeToAppRoot;
		private static readonly bool _isPackagedApp;
		private readonly ILogger _logger;

		static WinUIWebViewManager()
		{
			try
			{
				_isPackagedApp = Package.Current != null;
			}
			catch
			{
				_isPackagedApp = false;
			}
		}

		/// <summary>
		/// Initializes a new instance of <see cref="WinUIWebViewManager"/>
		/// </summary>
		/// <param name="webview">A <see cref="WebView2Control"/> to access platform-specific WebView2 APIs.</param>
		/// <param name="services">A service provider containing services to be used by this class and also by application code.</param>
		/// <param name="dispatcher">A <see cref="Dispatcher"/> instance that can marshal calls to the required thread or sync context.</param>
		/// <param name="fileProvider">Provides static content to the webview.</param>
		/// <param name="jsComponents">The <see cref="JSComponentConfigurationStore"/>.</param>
		/// <param name="contentRootRelativeToAppRoot">Path to the directory containing application content files.</param>
		/// <param name="hostPagePathWithinFileProvider">Path to the host page within the <paramref name="fileProvider"/>.</param>
		/// <param name="webViewHandler">The <see cref="BlazorWebViewHandler" />.</param>
		/// <param name="logger">Logger to send log messages to.</param>
		public WinUIWebViewManager(
			WebView2Control webview,
			IServiceProvider services,
			Dispatcher dispatcher,
			IFileProvider fileProvider,
			JSComponentConfigurationStore jsComponents,
			string contentRootRelativeToAppRoot,
			string hostPagePathWithinFileProvider,
			BlazorWebViewHandler webViewHandler,
			ILogger logger)
			: base(webview, services, dispatcher, fileProvider, jsComponents, contentRootRelativeToAppRoot, hostPagePathWithinFileProvider, webViewHandler, logger)
		{
			_handler = webViewHandler;
			_logger = logger;
			_webview = webview;
			_hostPageRelativePath = hostPagePathWithinFileProvider;
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;
		}

		/// <inheritdoc />
		protected override async Task HandleWebResourceRequest(CoreWebView2WebResourceRequestedEventArgs eventArgs)
		{
			var url = eventArgs.Request.Uri;

			_logger.LogDebug("Intercepting request for {Url}.", url);

			// 1. First check if the app wants to modify or override the request.
			if (WebRequestInterceptingWebView.TryInterceptResponseStream(_handler, _webview.CoreWebView2, eventArgs, url, _logger))
			{
				return;
			}

			// 2. If this is an app request, then assume the request is for a Blazor resource.
			var requestUri = QueryStringHelper.RemovePossibleQueryString(url);
			if (new Uri(requestUri) is Uri uri)
			{
				// Unlike server-side code, we get told exactly why the browser is making the request,
				// so we can be smarter about fallback. We can ensure that 'fetch' requests never result
				// in fallback, for example.
				var allowFallbackOnHostPage =
					eventArgs.ResourceContext == CoreWebView2WebResourceContext.Document ||
					eventArgs.ResourceContext == CoreWebView2WebResourceContext.Other; // e.g., dev tools requesting page source

				// Get a deferral object so that WebView2 knows there's some async stuff going on. We call Complete() at the end of this method.
				using var deferral = eventArgs.GetDeferral();

				_logger.HandlingWebRequest(requestUri);

				var relativePath = AppOriginUri.IsBaseOf(uri) ? AppOriginUri.MakeRelativeUri(uri).ToString() : null;

				// Check if the uri is _framework/blazor.modules.json is a special case as the built-in file provider
				// brings in a default implementation.
				if (relativePath != null &&
					string.Equals(relativePath, "_framework/blazor.modules.json", StringComparison.Ordinal) &&
					await TryServeFromFolderAsync(eventArgs, allowFallbackOnHostPage: false, requestUri, relativePath))
				{
					_logger.ResponseContentBeingSent(requestUri, 200);
				}
				else if (TryGetResponseContent(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers)
					&& statusCode != 404)
				{
					// First, call into WebViewManager to see if it has a framework file for this request. It will
					// fall back to an IFileProvider, but on WinUI it's always a NullFileProvider, so that will never
					// return a file.
					var headerString = GetHeaderString(headers);
					_logger.ResponseContentBeingSent(requestUri, statusCode);
					eventArgs.Response = _coreWebView2Environment!.CreateWebResourceResponse(content.AsRandomAccessStream(), statusCode, statusMessage, headerString);
				}
				else if (relativePath != null)
				{
					await TryServeFromFolderAsync(
						eventArgs,
						allowFallbackOnHostPage,
						requestUri,
						relativePath);
				}

				// Notify WebView2 that the deferred (async) operation is complete and we set a response.
				deferral.Complete();
				return;
			}

			// 3. If the request is not handled by the app nor is it a local source, then we let the WebView2
			//    handle the request as it would normally do. This means that it will try to load the resource
			//    from the internet or from the local cache.

			_logger.LogDebug("Request for {Url} was not handled.", url);
		}

		private async Task<bool> TryServeFromFolderAsync(
			CoreWebView2WebResourceRequestedEventArgs eventArgs,
			bool allowFallbackOnHostPage,
			string requestUri,
			string relativePath)
		{
			// If the path does not end in a file extension (or is empty), it's most likely referring to a page,
			// in which case we should allow falling back on the host page.
			if (allowFallbackOnHostPage && !Path.HasExtension(relativePath))
			{
				relativePath = _hostPageRelativePath;
			}
			relativePath = Path.Combine(_contentRootRelativeToAppRoot, relativePath.Replace('/', '\\'));
			var statusCode = 200;
			var statusMessage = "OK";
			var contentType = StaticContentProvider.GetResponseContentTypeOrDefault(relativePath);
			var headers = StaticContentProvider.GetResponseHeaders(contentType);
			IRandomAccessStream? stream = null;
			if (_isPackagedApp)
			{
				var winUIItem = await Package.Current.InstalledLocation.TryGetItemAsync(relativePath);
				var location = Package.Current.InstalledLocation.Path;
				if (winUIItem != null)
				{
					using var contentStream = await Package.Current.InstalledLocation.OpenStreamForReadAsync(relativePath);
					stream = await CopyContentToRandomAccessStreamAsync(contentStream);
				}
			}
			else
			{
				var path = Path.Combine(AppContext.BaseDirectory, relativePath);
				if (File.Exists(path))
				{
					using var contentStream = File.OpenRead(path);
					stream = await CopyContentToRandomAccessStreamAsync(contentStream);
				}
			}

			var hotReloadedContent = Stream.Null;
			if (StaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, requestUri, ref statusCode, ref hotReloadedContent, headers))
			{
				stream = await CopyContentToRandomAccessStreamAsync(hotReloadedContent);
			}

			if (stream != null)
			{
				var headerString = GetHeaderString(headers);

				_logger.ResponseContentBeingSent(requestUri, statusCode);

				eventArgs.Response = _coreWebView2Environment!.CreateWebResourceResponse(
					stream,
					statusCode,
					statusMessage,
					headerString);

				return true;
			}
			else
			{
				_logger.ResponseContentNotFound(requestUri);
			}

			return false;

			async Task<IRandomAccessStream> CopyContentToRandomAccessStreamAsync(Stream content)
			{
				using var memStream = new MemoryStream();
				await content.CopyToAsync(memStream);
				var randomAccessStream = new InMemoryRandomAccessStream();
				await randomAccessStream.WriteAsync(memStream.GetWindowsRuntimeBuffer());
				return randomAccessStream;
			}
		}

		/// <inheritdoc />
		protected override void QueueBlazorStart()
		{
			// In .NET MAUI we use autostart='false' for the Blazor script reference, so we start it up manually in this event
			_webview.CoreWebView2.DOMContentLoaded += async (_, __) =>
			{
				_logger.CallingBlazorStart();

				await _webview.CoreWebView2!.ExecuteScriptAsync(@"
					Blazor.start();
					");
			};
		}
	}
}
