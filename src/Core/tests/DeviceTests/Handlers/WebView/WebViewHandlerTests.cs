using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
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

		[Fact]
		public async Task WebViewPlaysHtml5Video()
		{
			var pageLoadTimeout = TimeSpan.FromSeconds(30);

			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub { 
					Width = 300,
					Height = 200,
				};
				var handler = CreateHandler(webView);
				var platformView = handler.PlatformView;

				
				await platformView.AttachAndRun(async () =>
				{
					var tcsLoaded = new TaskCompletionSource<bool>();
					var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML")));

					webView.NavigatedDelegate = (evnt, url, result) =>
					{
						if (result == WebNavigationResult.Success)
							tcsLoaded.TrySetResult(result == WebNavigationResult.Success);
					};

					// Load page
					webView.Source = new UrlWebViewSourceStub()
					{
						Url = "video.html"
					};
					handler.UpdateValue(nameof(IWebView.Source));

					// Ensure it loaded
					Assert.True(await tcsLoaded.Task, "HTML Source Failed to Load");

					// Wait for the color to appear
					var waited = TimeSpan.Zero;
					Exception lastException = null;

					var expectColorAfterStartsPlaying = Color.FromRgb(222, 255, 0);

					while (true)
					{
						var waitStep = TimeSpan.FromMilliseconds(2000);

						await Task.Delay(waitStep);
						waited += waitStep;

						try
						{
							var img = await webView.Capture();
							await img.AssertContainsColor(expectColorAfterStartsPlaying, new RectF(0,0,300,300));
							break;
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine(ex);
							lastException = ex;
						}

						if (waited >= pageLoadTimeout)
							throw lastException ?? new TimeoutException();
					}
				});
			});
		}
	}
}