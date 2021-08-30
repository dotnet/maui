using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.AspNetCore.Components.WebView.WebView2.Internal;
using Microsoft.Extensions.FileProviders;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// An implementation of <see cref="WebViewManager"/> that uses the Edge WebView2 browser control
	/// to render web content in WinUI applications.
	/// </summary>
	public class WinUIWebViewManager : WebView2WebViewManager
	{
		private readonly WebView2Control _nativeWebView2;
		private readonly string _hostPageRelativePath;
		private readonly string _contentRootDir;

		public WinUIWebViewManager(WebView2Control nativeWebView2, IWebView2Wrapper webview, IServiceProvider services, Dispatcher dispatcher, IFileProvider fileProvider, JSComponentConfigurationStore jsComponents, string hostPageRelativePath, string contentRootDir)
			: base(webview, services, dispatcher, fileProvider, jsComponents, hostPageRelativePath)
		{
			_nativeWebView2 = nativeWebView2;
			_hostPageRelativePath = hostPageRelativePath;
			_contentRootDir = contentRootDir;
		}

		protected override async Task HandleWebResourceRequest(ICoreWebView2WebResourceRequestedEventArgsWrapper eventArgs)
		{
			// Unlike server-side code, we get told exactly why the browser is making the request,
			// so we can be smarter about fallback. We can ensure that 'fetch' requests never result
			// in fallback, for example.
			var allowFallbackOnHostPage =
				eventArgs.ResourceContext == CoreWebView2WebResourceContextWrapper.Document ||
				eventArgs.ResourceContext == CoreWebView2WebResourceContextWrapper.Other; // e.g., dev tools requesting page source

			// Get a deferral object so that WebView2 knows there's some async stuff going on. We call Complete() at the end of this method.
			using var deferral = eventArgs.GetDeferral();

			// First, call into WebViewManager to see if it has a framework file for this request. It will
			// fall back to an IFileProvider, but on WinUI it's always a NullFileProvider, so that will never
			// return a file.
			if (TryGetResponseContent(eventArgs.Request.Uri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers)
				&& statusCode != 404)
			{
				// NOTE: This is stream copying is to work around a hanging bug in WinRT with managed streams.
				// See issue https://github.com/microsoft/CsWinRT/issues/670
				var memStream = new MemoryStream();
				content.CopyTo(memStream);
				var ms = new InMemoryRandomAccessStream();
				await ms.WriteAsync(memStream.GetWindowsRuntimeBuffer());

				var headerString = GetHeaderString(headers);
				eventArgs.SetResponse(ms, statusCode, statusMessage, headerString);
			}
			else
			{
				// Next, try to go through WinUI Storage to find a static web asset
				var uri = new Uri(eventArgs.Request.Uri);
				if (new Uri(AppOrigin).IsBaseOf(uri))
				{
					var relativePath = new Uri(AppOrigin).MakeRelativeUri(uri).ToString();
					if (allowFallbackOnHostPage && string.IsNullOrEmpty(relativePath))
					{
						relativePath = _hostPageRelativePath;
					}
					relativePath = Path.Combine("Assets", _contentRootDir, relativePath.Replace("/", "\\"));

					var winUIItem = await Package.Current.InstalledLocation.TryGetItemAsync(relativePath);
					if (winUIItem != null)
					{
						statusCode = 200;
						statusMessage = "OK";
						var contentType = StaticContentProvider.GetResponseContentTypeOrDefault(relativePath);
						headers = StaticContentProvider.GetResponseHeaders(contentType);
						var headerString = GetHeaderString(headers);
						var winUIFile = await Package.Current.InstalledLocation.GetFileAsync(relativePath);

						eventArgs.SetResponse(await winUIFile.OpenReadAsync(), statusCode, statusMessage, headerString);
					}
				}
			}

			// Notify WebView2 that the deferred (async) operation is complete and we set a response.
			deferral.Complete();
		}

		protected override void QueueBlazorStart()
		{
			// In .NET MAUI we use autostart='false' for the Blazor script reference, so we start it up manually in this event
			_nativeWebView2.CoreWebView2.DOMContentLoaded += async (_, __) =>
			{
				await _nativeWebView2.CoreWebView2!.ExecuteScriptAsync(@"
					Blazor.start();
					");
			};
		}
	}
}
