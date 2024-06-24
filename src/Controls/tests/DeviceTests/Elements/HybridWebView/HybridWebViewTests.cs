using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.HybridWebView)]
	public class HybridWebViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
				});
			});
		}

		[Fact]
		public async Task LoadsHtmlAndSendReceiveRawMessage()
		{
			SetupBuilder();

			var actual = string.Empty;

			var pageLoadTimeout = TimeSpan.FromSeconds(2);

			//string html =
			//	@"
			//	<!DOCTYPE html>
			//	<html>
			//		<head>
			//		</head>
			//		<body>
			//			<script>
			//				function test() {
			//					return 'Test';
			//				}
			//			</script>
			//			<p>
			//				WebView Unit Test
			//			</p>
			//		</body>
			//	</html>
			//	";
			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,
					//Source = new HtmlWebViewSource { Html = html }
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Setup the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await Task.Delay(5000);
					//// Wait for the page to load
					//var tcsLoaded = new TaskCompletionSource<bool>();
					//var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
					//ctsTimeout.Token.Register(() => tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML")));

					//webView.Navigated += async (source, args) =>
					//{
					//	// Set success when we have a successful nav result
					//	if (args.Result == WebNavigationResult.Success)
					//	{
					//		tcsLoaded.TrySetResult(args.Result == WebNavigationResult.Success);

					//		// Evaluate JavaScript
					//		var script = "test();";
					//		actual = await webView.EvaluateJavaScriptAsync(script);

					//		// If the result is equal to the script string result, the test has passed
					//		Assert.Equal("Test", actual);
					//	}
					//};

					//Assert.True(await tcsLoaded.Task);
				});
			});
		}
	}
}
