using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.WebView)]
#if WINDOWS
	[Collection(WebViewsCollection)]
#endif
	public partial class WebViewTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Evaluate JavaScript returning a String value"
#if WINDOWS
		, Skip = "Fails on Windows"
#endif
		)]
		public async Task EvaluateJavaScriptWithString()
		{
			SetupBuilder();

			string actual = string.Empty;

			var pageLoadTimeout = TimeSpan.FromSeconds(2);

			string html =
				@"
				<!DOCTYPE html>
				<html>
					<head>
					</head>
					<body>
						<script>
							function test() {
								return 'Test';
							}
						</script>
						<p>
							WebView Unit Test
						</p>
					</body>
				</html>
				";
			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,
					Source = new HtmlWebViewSource { Html = html }
				};

				var handler = CreateHandler(webView);

				var platformView = handler.PlatformView;

				// Setup the view to be displayed/parented and run our tests on it
				await AttachAndRun(webView, async (handler) =>
				{
					// Wait for the page to load
					var tcsLoaded = new TaskCompletionSource<bool>();
					var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML")));

					webView.Navigated += async (source, args) =>
					{
						// Set success when we have a successful nav result
						if (args.Result == WebNavigationResult.Success)
						{
							tcsLoaded.TrySetResult(args.Result == WebNavigationResult.Success);

							// Evaluate JavaScript
							var script = "test();";
							actual = await webView.EvaluateJavaScriptAsync(script);

							// If the result is equal to the script string result, the test has passed
							Assert.Equal("Test", actual);
						}
					};

					Assert.True(await tcsLoaded.Task);
				});
			});
		}

		[Fact(DisplayName = "Evaluate JavaScript returning an Integer value"
#if WINDOWS
		, Skip = "Fails on Windows"
#endif
		)]
		public async Task EvaluateJavaScriptWithInteger()
		{
			SetupBuilder();

			string actual = string.Empty;

			var pageLoadTimeout = TimeSpan.FromSeconds(2);

			string html =
				@"
				<!DOCTYPE html>
				<html>
					<head>
					</head>
					<body>
						<script>
							function test() {
								return 10;
							}
						</script>
						<p>
							WebView Unit Test
						</p>
					</body>
				</html>
				";
			await InvokeOnMainThreadAsync(async () =>
			{
				var webView = new WebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,
					Source = new HtmlWebViewSource { Html = html }
				};

				var handler = CreateHandler(webView);

				var platformView = handler.PlatformView;

				// Setup the view to be displayed/parented and run our tests on it
				await AttachAndRun(webView, async (handler) =>
				{
					// Wait for the page to load
					var tcsLoaded = new TaskCompletionSource<bool>();
					var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML")));

					webView.Navigated += async (source, args) =>
					{
						// Set success when we have a successful nav result
						if (args.Result == WebNavigationResult.Success)
						{
							tcsLoaded.TrySetResult(args.Result == WebNavigationResult.Success);

							// Evaluate JavaScript
							var script = "test();";
							actual = await webView.EvaluateJavaScriptAsync(script);

							// If the result is equal to the script string result, the test has passed
							Assert.Equal(10, Convert.ToInt32(actual));
						}
					};

					Assert.True(await tcsLoaded.Task);
				});
			});
		}
	}
}
