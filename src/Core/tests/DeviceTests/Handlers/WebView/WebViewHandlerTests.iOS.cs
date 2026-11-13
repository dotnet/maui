using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using WebKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		WKWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url.AbsoluteString;

		[Theory(DisplayName = "HtmlWebViewSource BaseUrl With A Scheme Is The Document Base URL")]
		[InlineData("https://example.com/")]
		[InlineData("https://example.com/content/")]
		public async Task HtmlSourceBaseUrlWithSchemeIsDocumentBaseUrl(string baseUrl)
		{
			var documentBaseUrl = await LoadHtmlAndGetDocumentBaseUrl(baseUrl);

			Assert.Equal(baseUrl, documentBaseUrl);
		}

		[Fact(DisplayName = "HtmlWebViewSource BaseUrl That Is A File Path Stays A File URL")]
		public async Task HtmlSourceBaseUrlFilePathStaysFileUrl()
		{
			var bundlePath = NSBundle.MainBundle.BundlePath;

			var documentBaseUrl = await LoadHtmlAndGetDocumentBaseUrl(bundlePath);

			Assert.Equal(new NSUrl(bundlePath, true).AbsoluteString, documentBaseUrl);
		}

		async Task<string> LoadHtmlAndGetDocumentBaseUrl(string baseUrl)
		{
			var pageLoadTimeout = TimeSpan.FromSeconds(30);
			string documentBaseUrl = null;

			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub()
				{
					Width = 100,
					Height = 100,
				};

				var handler = CreateHandler(webView);

				await AttachAndRun(webView, async (handler) =>
				{
					var tcsLoaded = new TaskCompletionSource<bool>();
					using var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException("Failed to load HTML")));

					webView.NavigatedDelegate = (evnt, url, result) =>
					{
						if (result == WebNavigationResult.Success)
							tcsLoaded.TrySetResult(true);
					};

					webView.Source = new HtmlWebViewSourceStub
					{
						Html = "<html><body><p>BaseUrl</p></body></html>",
						BaseUrl = baseUrl,
					};
					handler.UpdateValue(nameof(IWebView.Source));

					Assert.True(await tcsLoaded.Task, "HTML Source Failed to Load");

					var result = await handler.PlatformView.EvaluateJavaScriptAsync("document.baseURI");
					documentBaseUrl = result?.ToString();
				});
			});

			return documentBaseUrl;
		}
	}
}