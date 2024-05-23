using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Streams;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, WebView2>
	{
		protected override WebView2 CreatePlatformView()
		{
#if DEBUG
			var logger = MauiContext!.Services!.GetService<ILogger<HybridWebViewHandler>>() ?? NullLogger<HybridWebViewHandler>.Instance;
			logger.LogInformation("HybridWebViewHandler: CreatePlatformView WebView2");
#endif

			return new MauiHybridWebView(this);
		}

		protected override async void ConnectHandler(WebView2 platformView)
		{
#if DEBUG
			var logger = MauiContext!.Services!.GetService<ILogger<HybridWebViewHandler>>() ?? NullLogger<HybridWebViewHandler>.Instance;
			logger.LogInformation("HybridWebViewHandler: Connecting WebView2");
#endif

			base.ConnectHandler(platformView);


			platformView.WebMessageReceived += OnWebMessageReceived;

			await platformView.EnsureCoreWebView2Async();

			platformView.CoreWebView2.Settings.AreDevToolsEnabled = true;//EnableWebDevTools;
			platformView.CoreWebView2.Settings.IsWebMessageEnabled = true;
			platformView.CoreWebView2.AddWebResourceRequestedFilter($"{AppOrigin}*", CoreWebView2WebResourceContext.All);
			platformView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;


			platformView.Source = new Uri(new Uri(AppOriginUri, "/").ToString());

		}

		protected override void DisconnectHandler(WebView2 platformView)
		{
#if DEBUG
			var logger = MauiContext!.Services!.GetService<ILogger<HybridWebViewHandler>>() ?? NullLogger<HybridWebViewHandler>.Instance;
			logger.LogInformation("HybridWebViewHandler: Disconnecting WebView2");
#endif

			platformView.WebMessageReceived -= OnWebMessageReceived;
			platformView.Close();

			base.DisconnectHandler(platformView);
		}

		public static void MapSendRawMessage(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
			if (arg is not string rawMessage || handler.PlatformView is not IHybridPlatformWebView hybridPlatformWebView)
			{
				return;
			}

			hybridPlatformWebView.SendRawMessage(rawMessage);
		}


		private void OnWebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
		{
			VirtualView?.RawMessageReceived(args.TryGetWebMessageAsString());
		}

		private async void CoreWebView2_WebResourceRequested(CoreWebView2 sender, CoreWebView2WebResourceRequestedEventArgs eventArgs)
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
	}
}
