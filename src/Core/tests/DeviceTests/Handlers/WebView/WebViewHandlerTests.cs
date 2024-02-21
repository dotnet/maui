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

		[Fact("WebBrowser autoplays HTML5 Video"
#if ANDROID || IOS || MACCATALYST || WINDOWS
			, Skip = "Capturing a screenshot/image of a WebView does not also capture the video canvas contents required for this test."
#endif
			)]
		public async Task WebViewPlaysHtml5Video()
		{
			var pageLoadTimeout = TimeSpan.FromSeconds(30);
			var expectedColorInPlayingVideo = Color.FromRgb(222, 255, 0);

			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebViewStub
				{
					Width = 300,
					Height = 200,
				};
				var handler = CreateHandler(webView);

				// Get the platform view (ensure it's created)
				var platformView = handler.PlatformView;

				Exception lastException = null;

				// Setup the view to be displayed/parented and run our tests on it
				await AttachAndRun(webView, async (handler) =>
				{
					// Wait for the page to load
					var tcsLoaded = new TaskCompletionSource<bool>();
					var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML")));
					webView.NavigatedDelegate = (evnt, url, result) =>
					{
						// Set success when we have a successful nav result
						if (result == WebNavigationResult.Success)
							tcsLoaded.TrySetResult(result == WebNavigationResult.Success);
					};

					// Load page
					webView.Source = new UrlWebViewSourceStub
					{
						Url = "video.html"
					};
					handler.UpdateValue(nameof(IWebView.Source));

					// Ensure it loaded, the task completion source gets set when load or failed
					Assert.True(await tcsLoaded.Task, "HTML Source Failed to Load");

					var maxAttempts = 3;
					var attempts = 0;

					while (attempts <= maxAttempts)
					{
						attempts++;
						await Task.Delay(2000);

						try
						{
							// Capture a screenshot from the webview
							var img = await webView.Capture();

							// This color is expected to appear in the top left corner of the video
							// after the video has played for a couple seconds
							await img.AssertContainsColor(expectedColorInPlayingVideo, imageRect =>
								new Graphics.RectF(
									imageRect.Width * 0.15f,
									imageRect.Height * 0.15f,
									imageRect.Width * 0.35f,
									imageRect.Height * 0.35f));

							// If the assertion passes, the test succeeded
							return;
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine(ex);
							lastException = ex;
						}
					}

					throw lastException ?? new Exception();
				});
			});
		}
	}
}