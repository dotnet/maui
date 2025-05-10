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

		[Theory(DisplayName = "WebView loads non-Western character encoded URLs correctly"
#if WINDOWS
			, Skip = "Skipping this test on Windows due to WebView's OnNavigated event returning WebNavigationResult.Failure for URLs with non-Western characters. More information: https://github.com/dotnet/maui/issues/27425"
#endif
		)]
		[InlineData("https://example.com/test-Ağ-Sistem%20Bilgi%20Güvenliği%20Md/Guide.pdf")] // Non-ASCII character + space (%20) (Outside IRI range)
		[InlineData("https://google.com/[]")] // Reserved set (`;/?:@&=+$,#[]!'()*%`)
		[InlineData("https://example.com/test/%3Cvalue%3E")] // Escaped character from " <>`^{|} set (e.g., < >)
		[InlineData("https://example.com/path/%09text")] // Escaped character from [0, 1F] range (e.g., tab %09)
		[InlineData("https://example.com/test?query=%26value")] // Another escaped character from reserved set (e.g., & as %26)
		public async Task WebViewShouldLoadEncodedUrl(string encodedUrl)
		{
			if (await AssertionExtensions.SkipTestIfNoInternetConnection())
			{
				return;
			}
			var webView = new WebView();
			var tcs = new TaskCompletionSource<WebNavigationResult>();

			webView.Navigated += (sender, args) =>
			{
				tcs.TrySetResult(args.Result);
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<WebViewHandler>(webView);
				(handler.PlatformView as IWebViewDelegate)?.LoadUrl(encodedUrl);
			});

			var navigationResult = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Equal(WebNavigationResult.Success, navigationResult);
		}
	}
}