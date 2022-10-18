using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Extensions.FileProviders;
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
		private readonly WebView2Control _webview;
		private readonly string _hostPageRelativePath;
		private readonly string _contentRootRelativeToAppRoot;
		private static readonly bool _isPackagedApp;

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
		public WinUIWebViewManager(
			WebView2Control webview,
			IServiceProvider services,
			Dispatcher dispatcher,
			IFileProvider fileProvider,
			JSComponentConfigurationStore jsComponents,
			string contentRootRelativeToAppRoot,
			string hostPagePathWithinFileProvider,
			BlazorWebViewHandler webViewHandler)
			: base(webview, services, dispatcher, fileProvider, jsComponents, contentRootRelativeToAppRoot, hostPagePathWithinFileProvider, webViewHandler)
		{
			_webview = webview;
			_hostPageRelativePath = hostPagePathWithinFileProvider;
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;
		}

		/// <inheritdoc />
		protected override async Task HandleWebResourceRequest(CoreWebView2WebResourceRequestedEventArgs eventArgs)
		{
			// Unlike server-side code, we get told exactly why the browser is making the request,
			// so we can be smarter about fallback. We can ensure that 'fetch' requests never result
			// in fallback, for example.
			var allowFallbackOnHostPage =
				eventArgs.ResourceContext == CoreWebView2WebResourceContext.Document ||
				eventArgs.ResourceContext == CoreWebView2WebResourceContext.Other; // e.g., dev tools requesting page source

			// Get a deferral object so that WebView2 knows there's some async stuff going on. We call Complete() at the end of this method.
			using var deferral = eventArgs.GetDeferral();

			var requestUri = QueryStringHelper.RemovePossibleQueryString(eventArgs.Request.Uri);

			// First, call into WebViewManager to see if it has a framework file for this request. It will
			// fall back to an IFileProvider, but on WinUI it's always a NullFileProvider, so that will never
			// return a file.
			if (TryGetResponseContent(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers)
				&& statusCode != 404)
			{
				var headerString = GetHeaderString(headers);
				eventArgs.Response = _coreWebView2Environment!.CreateWebResourceResponse(content.AsRandomAccessStream(), statusCode, statusMessage, headerString);
			}
			else if (new Uri(requestUri) is Uri uri && AppOriginUri.IsBaseOf(uri))
			{
				var relativePath = AppOriginUri.MakeRelativeUri(uri).ToString();

				// If the path does not end in a file extension (or is empty), it's most likely referring to a page,
				// in which case we should allow falling back on the host page.
				if (allowFallbackOnHostPage && !Path.HasExtension(relativePath))
				{
					relativePath = _hostPageRelativePath;
				}
				relativePath = Path.Combine(_contentRootRelativeToAppRoot, relativePath.Replace('/', '\\'));
				statusCode = 200;
				statusMessage = "OK";
				var contentType = StaticContentProvider.GetResponseContentTypeOrDefault(relativePath);
				headers = StaticContentProvider.GetResponseHeaders(contentType);
				IRandomAccessStream? stream = null;
				if (_isPackagedApp)
				{
					var winUIItem = await Package.Current.InstalledLocation.TryGetItemAsync(relativePath);
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
					eventArgs.Response = _coreWebView2Environment!.CreateWebResourceResponse(
						stream,
						statusCode,
						statusMessage,
						headerString);
				}

				async Task<IRandomAccessStream> CopyContentToRandomAccessStreamAsync(Stream content)
				{
					using var memStream = new MemoryStream();
					await content.CopyToAsync(memStream);
					var randomAccessStream = new InMemoryRandomAccessStream();
					await randomAccessStream.WriteAsync(memStream.GetWindowsRuntimeBuffer());
					return randomAccessStream;
				}
			}

			// Notify WebView2 that the deferred (async) operation is complete and we set a response.
			deferral.Complete();
		}

		/// <inheritdoc />
		protected override void QueueBlazorStart()
		{
			// In .NET MAUI we use autostart='false' for the Blazor script reference, so we start it up manually in this event
			_webview.CoreWebView2.DOMContentLoaded += async (_, __) =>
			{
				await _webview.CoreWebView2!.ExecuteScriptAsync(@"
					Blazor.start();
					");
			};
		}
	}
}
