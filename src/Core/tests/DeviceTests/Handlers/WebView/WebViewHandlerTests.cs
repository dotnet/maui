using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.WebView)]
	public partial class WebViewHandlerTests : CoreHandlerTestBase<WebViewHandler, WebViewStub>
	{
		[Theory(DisplayName = "UrlSource Initializes Correctly")]
		[InlineData("https://dotnet.microsoft.com/")]
		[InlineData("https://devblogs.microsoft.com/dotnet/")]
		[InlineData("https://xamarin.com/")]
		public async Task UrlSourceInitializesCorrectly(string urlSource)
		{
			var webView = new WebViewStub()
			{
				Source = new UrlWebViewSourceStub { Url = urlSource }
			};

			var url = ((UrlWebViewSourceStub)webView.Source).Url;

			await InvokeOnMainThreadAsync(() => ValidatePropertyInitValue(webView, () => url, GetNativeSource, url));
		}

		[Theory]
		[InlineData("row-video.element")]
		public async Task WebViewIsHtml5Compatible(params string[] elementIds)
		{
			var pageLoadTimeout = TimeSpan.FromSeconds(30);
			var url = "https://html5test.com/";

			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub { 
					Width = 500,
					Height = 500,
				};
				var handler = CreateHandler(webView);
				var platformView = handler.PlatformView;

				
				await platformView.AttachAndRun(async () =>
				{
					var tcsLoaded = new TaskCompletionSource<bool>();
					var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException($"Failed to load {url}")));

					webView.NavigatedDelegate = (evnt, url, result) =>
					{
						if (url.Equals(url, System.StringComparison.OrdinalIgnoreCase))
							tcsLoaded.TrySetResult(result == WebNavigationResult.Success);
					};

					// Load page
					webView.Source = new UrlWebViewSourceStub() { Url = url };
					handler.UpdateValue(nameof(IWebView.Source));
					
					// Ensure it loaded
					Assert.True(await tcsLoaded.Task, "HTML5Test Page Failed to Load");

					// Wait for the DOM to settle by polling for a known element to exist
					var waited = TimeSpan.Zero;
					var waitIncrement = TimeSpan.FromSeconds(1);

					while (waited < pageLoadTimeout)
					{
						// This is the element we want to wait to see to know 
						var scoreTxt = await webView.EvaluateJavaScriptAsync($"document.getElementById('score').innerText");
						if (!string.IsNullOrWhiteSpace(scoreTxt))
							break;
						await Task.Delay(waitIncrement);
						waited += waitIncrement;
					}

					// Assert that all the requested elements exist by id and contain a passing check mark
					foreach (var elementId in elementIds)
					{
						var txt = await webView.EvaluateJavaScriptAsync($"document.getElementById('{elementId}').innerText");
						Assert.Contains("✔", txt, System.StringComparison.InvariantCultureIgnoreCase);
					}
				});
			});
		}
	}
}